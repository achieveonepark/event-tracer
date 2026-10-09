using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Achieve.EventTracer.Editor
{
    public sealed class EventTraceWindow : EditorWindow
    {
        private readonly List<TraceRecord> visible = new List<TraceRecord>();
        private readonly Dictionary<TraceId, TraceRecord> byId = new Dictionary<TraceId, TraceRecord>();
        private TraceCapture displayed;
        private TraceCapture imported;
        private TraceId selectedId;
        private long lastVersion = -1;
        private bool paused;
        private bool updatingFilters;
        private ListView list;
        private ToolbarSearchField search;
        private ToolbarSearchField sourceSearch;
        private DropdownField category;
        private DropdownField level;
        private Toggle recording;
        private Button pauseButton;
        private Button liveButton;
        private Label sessionTitle;
        private Label summary;
        private Label modeBadge;
        private Label empty;
        private VisualElement details;
        private HelpBox notice;

        [MenuItem("Tools/Achieve/Event Tracer")]
        public static void Open() => GetWindow<EventTraceWindow>("Event Tracer");

        public static void ShowCapture(TraceCapture capture)
        {
            var window = GetWindow<EventTraceWindow>("Event Tracer");
            window.LoadCapture(capture);
        }

        public void LoadCapture(TraceCapture capture)
        {
            imported = capture ?? throw new ArgumentNullException(nameof(capture));
            displayed = imported;
            selectedId = default;
            Refresh(true);
        }

        public void CreateGUI()
        {
            minSize = new Vector2(1000, 600);
            var root = rootVisualElement;
            root.Clear();
            root.AddToClassList("et-root");
            if (!EditorGUIUtility.isProSkin) root.AddToClassList("light-theme");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.achive.event-tracer/Editor/UI/Styles/EventTracer.uss");
            if (style != null) root.styleSheets.Add(style);

            var header = new Toolbar();
            header.AddToClassList("et-app-header");
            var wordmark = new Label("Event Tracer");
            wordmark.AddToClassList("et-wordmark");
            header.Add(wordmark);
            var tagline = new Label("Recorded events and causes");
            tagline.AddToClassList("et-header-hint");
            header.Add(tagline);
            header.Add(TraceLocalization.CreateSelector());
            root.Add(header);

            var sessionBar = new Toolbar();
            sessionBar.AddToClassList("et-session-bar");
            var sessionInfo = new VisualElement();
            sessionInfo.style.flexGrow = 1;
            sessionTitle = new Label("Play session");
            sessionTitle.AddToClassList("et-session-title");
            sessionTitle.AddToClassList("achieve-data");
            summary = new Label();
            summary.AddToClassList("et-muted");
            sessionInfo.Add(sessionTitle);
            sessionInfo.Add(summary);
            sessionBar.Add(sessionInfo);
            modeBadge = new Label("LIVE");
            modeBadge.AddToClassList("et-mode-badge");
            sessionBar.Add(modeBadge);
            recording = new Toggle("Record") { value = EventTrace.IsEnabled, name = "trace-recording" };
            recording.AddToClassList("et-record-toggle");
            recording.RegisterValueChangedCallback(evt =>
            {
                var options = EventTrace.Configuration;
                options.enabled = evt.newValue;
                TraceEditorPreferences.Save(options);
            });
            sessionBar.Add(recording);
            pauseButton = AddButton(sessionBar, "Pause view", TogglePause, "trace-pause");
            AddButton(sessionBar, "New session", () => { EventTrace.StartSession("Manual session"); ShowLive(); }, "trace-new-session");
            AddButton(sessionBar, "Clear", () => { EventTrace.Clear(); ShowLive(); }, "trace-clear");
            AddButton(sessionBar, "Export", Export, "trace-export");
            AddButton(sessionBar, "Import", Import, "trace-import");
            root.Add(sessionBar);

            notice = new HelpBox(string.Empty, HelpBoxMessageType.Error) { name = "trace-notice" };
            notice.style.display = DisplayStyle.None;
            root.Add(notice);
            var filters = new VisualElement();
            filters.AddToClassList("et-filter-bar");
            search = new ToolbarSearchField { name = "trace-search", tooltip = "Search event names and payloads" };
            search.AddToClassList("et-search");
            search.RegisterValueChangedCallback(evt => SetSearchFilter(evt.newValue));
            filters.Add(search);
            category = new DropdownField(new List<string> { "All categories" }, 0) { name = "trace-category" };
            category.AddToClassList("et-category-filter");
            category.formatListItemCallback = TraceLocalization.Text;
            category.formatSelectedValueCallback = TraceLocalization.Text;
            category.RegisterValueChangedCallback(_ => { if (!updatingFilters) Refilter(); });
            filters.Add(category);
            level = new DropdownField(new List<string> { "All levels", "Info", "Warning", "Error" }, 0) { name = "trace-level" };
            level.AddToClassList("et-level-filter");
            level.formatListItemCallback = TraceLocalization.Text;
            level.formatSelectedValueCallback = TraceLocalization.Text;
            level.RegisterValueChangedCallback(_ => Refilter());
            filters.Add(level);
            var sourceLabel = new Label("Source");
            sourceLabel.AddToClassList("et-muted");
            filters.Add(sourceLabel);
            sourceSearch = new ToolbarSearchField { name = "trace-source", tooltip = "Filter by source object name" };
            sourceSearch.AddToClassList("et-source-filter");
            sourceSearch.RegisterValueChangedCallback(_ => Refilter());
            filters.Add(sourceSearch);
            liveButton = AddButton(filters, "Back to live", ShowLive, "trace-live");
            root.Add(filters);

            var split = new TwoPaneSplitView(1, 320, TwoPaneSplitViewOrientation.Horizontal);
            split.AddToClassList("et-workspace");
            var eventPanel = new VisualElement();
            eventPanel.AddToClassList("et-event-panel");
            var columns = new VisualElement();
            columns.AddToClassList("et-column-header");
            AddColumn(columns, "TIME", "et-time");
            AddColumn(columns, "FRAME", "et-frame");
            AddColumn(columns, "EVENT / CAUSE", "et-event-name");
            AddColumn(columns, "SOURCE", "et-source");
            eventPanel.Add(columns);
            list = new ListView(visible, 26, MakeEventRow, BindEventRow)
            {
                name = "trace-event-list", selectionType = SelectionType.Single
            };
            list.AddToClassList("et-event-list");
            list.selectionChanged += objects =>
            {
                var record = objects.OfType<TraceRecord>().FirstOrDefault();
                if (record == null) return;
                selectedId = record.Id;
                ShowDetails(record);
            };
            eventPanel.Add(list);
            empty = new Label();
            empty.AddToClassList("et-empty-events");
            eventPanel.Add(empty);
            split.Add(eventPanel);
            var detailScroll = new ScrollView { name = "trace-details" };
            detailScroll.AddToClassList("et-detail-panel");
            details = new VisualElement();
            detailScroll.Add(details);
            split.Add(detailScroll);
            root.Add(split);
            root.Add(CreateLimits());
            Refresh(true);
            root.schedule.Execute(() => Refresh(false)).Every(150);
            TraceLocalization.Install(root);
        }

        private static Button AddButton(VisualElement parent, string text, Action callback, string name = null)
        {
            var button = new Button(callback) { text = text, name = name };
            button.AddToClassList("et-button");
            parent.Add(button);
            return button;
        }

        public void SetSearchFilter(string query)
        {
            if (search == null) throw new InvalidOperationException("Create the window UI before setting its filter.");
            search.SetValueWithoutNotify(query ?? string.Empty);
            Refilter();
        }

        private static void AddColumn(VisualElement parent, string title, string className)
        {
            var label = new Label(title);
            label.AddToClassList(className);
            if (string.IsNullOrEmpty(title)) label.AddToClassList("achieve-data");
            parent.Add(label);
        }

        private VisualElement CreateLimits()
        {
            var foldout = new Foldout { text = "Capture limits", value = false };
            foldout.AddToClassList("et-limits");
            var options = EventTrace.Configuration;
            var row = new VisualElement();
            row.AddToClassList("et-limits-row");
            var maxEvents = new IntegerField("Events") { value = options.maxEvents, name = "trace-max-events" };
            var payload = new IntegerField("Payload characters") { value = options.maxPayloadCharacters };
            var budget = new IntegerField("Buffer characters") { value = options.maxBufferCharacters };
            var caller = new Toggle("Capture caller") { value = options.captureCaller };
            row.Add(maxEvents); row.Add(payload); row.Add(budget); row.Add(caller);
            AddButton(row, "Apply", () =>
            {
                try
                {
                    var config = EventTrace.Configuration;
                    config.maxEvents = maxEvents.value;
                    config.maxPayloadCharacters = payload.value;
                    config.maxBufferCharacters = budget.value;
                    config.captureCaller = caller.value;
                    TraceEditorPreferences.Save(config);
                    notice.style.display = DisplayStyle.None;
                    Refresh(true);
                }
                catch (Exception exception) { ShowError(exception.Message); }
            });
            foldout.Add(row);
            return foldout;
        }

        private VisualElement MakeEventRow()
        {
            var row = new VisualElement();
            row.AddToClassList("et-event-row");
            AddColumn(row, string.Empty, "et-time");
            AddColumn(row, string.Empty, "et-frame");
            var nameGroup = new VisualElement();
            nameGroup.AddToClassList("et-event-name");
            var dot = new Label("●");
            dot.AddToClassList("et-level-dot");
            nameGroup.Add(dot);
            var eventLabel = new Label { name = "event-name" };
            eventLabel.AddToClassList("achieve-data");
            eventLabel.AddToClassList("et-event-label");
            nameGroup.Add(eventLabel);
            row.Add(nameGroup);
            AddColumn(row, string.Empty, "et-source");
            return row;
        }

        private void BindEventRow(VisualElement row, int index)
        {
            var record = visible[index];
            row.Q<Label>(className: "et-time").text = $"{record.ElapsedSeconds:0.000}s";
            row.Q<Label>(className: "et-frame").text = record.Frame.ToString();
            row.Q<Label>("event-name").text = record.EventName;
            row.Q<Label>(className: "et-source").text = record.SourceName;
            var group = row.Q(className: "et-event-name");
            group.style.paddingLeft = Depth(record) * 12;
            var dot = row.Q<Label>(className: "et-level-dot");
            dot.EnableInClassList("is-warning", record.Level == TraceLevel.Warning);
            dot.EnableInClassList("is-error", record.Level == TraceLevel.Error);
            row.tooltip = $"{record.Category} · #{record.Id.Sequence}\n{record.PayloadJson}";
        }

        private int Depth(TraceRecord record)
        {
            var depth = 0;
            while (depth < 8 && record.ParentId.IsValid && byId.TryGetValue(record.ParentId, out var parent))
            {
                depth++;
                record = parent;
            }
            return depth;
        }

        private void Refresh(bool force)
        {
            if (list == null) return;
            recording.SetValueWithoutNotify(EventTrace.IsEnabled);
            if (imported == null && !paused && (force || lastVersion != EventTrace.Version))
            {
                displayed = EventTrace.Capture();
                lastVersion = EventTrace.Version;
                RebuildCategories();
                Refilter();
            }
            else if (force)
            {
                displayed = imported ?? displayed ?? EventTrace.Capture();
                RebuildCategories();
                Refilter();
            }
            if (displayed == null) return;
            sessionTitle.text = displayed.Session.Name;
            TraceLocalization.SetText(summary, $"{visible.Count:N0} shown · {displayed.Records.Count:N0} captured · {displayed.DroppedCount:N0} dropped");
            summary.EnableInClassList("is-warning", displayed.DroppedCount > 0);
            TraceLocalization.SetText(modeBadge, imported != null ? "IMPORTED" : paused ? "PAUSED VIEW" : "LIVE");
            modeBadge.EnableInClassList("is-muted", imported != null || paused);
            TraceLocalization.SetText(pauseButton, paused ? "Resume view" : "Pause view");
            pauseButton.SetEnabled(imported == null);
            liveButton.style.display = imported != null || paused ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void RebuildCategories()
        {
            if (displayed == null) return;
            updatingFilters = true;
            var previous = category.value;
            var choices = new List<string> { "All categories" };
            choices.AddRange(displayed.Records.Select(record => record.Category).Where(value => !string.IsNullOrEmpty(value))
                .Distinct().OrderBy(value => value, StringComparer.Ordinal));
            category.choices = choices;
            category.SetValueWithoutNotify(choices.Contains(previous) ? previous : choices[0]);
            updatingFilters = false;
        }

        private void Refilter()
        {
            if (list == null || displayed == null) return;
            byId.Clear();
            foreach (var record in displayed.Records) byId[record.Id] = record;
            visible.Clear();
            var query = search.value ?? string.Empty;
            var sourceQuery = sourceSearch.value ?? string.Empty;
            foreach (var record in displayed.Records)
            {
                if (category.index > 0 && record.Category != category.value) continue;
                if (level.index > 0 && record.Level.ToString() != level.value) continue;
                if (query.Length > 0 && !Contains(record.EventName, query) && !Contains(record.PayloadJson, query)) continue;
                if (sourceQuery.Length > 0 && !Contains(record.SourceName, sourceQuery)) continue;
                visible.Add(record);
            }
            list.Rebuild();
            var selectedIndex = visible.FindIndex(record => record.Id == selectedId);
            list.SetSelectionWithoutNotify(selectedIndex >= 0 ? new[] { selectedIndex } : Array.Empty<int>());
            if (byId.TryGetValue(selectedId, out var selected)) ShowDetails(selected);
            else ShowDetails(null);
            TraceLocalization.SetText(empty, displayed.Records.Count == 0
                ? "Waiting for your first event.\nRun your game or try the Shop Demo."
                : "No events match these filters.");
            empty.style.display = visible.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            list.style.display = visible.Count == 0 ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private static bool Contains(string value, string query) => value != null &&
            value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

        private void ShowDetails(TraceRecord record)
        {
            details.Clear();
            if (record == null)
            {
                var welcome = new VisualElement();
                welcome.AddToClassList("et-detail-empty");
                var title = new Label("No event selected");
                title.AddToClassList("et-detail-title");
                welcome.Add(title);
                var body = new Label("Select an event to inspect its data, source, and recorded cause.");
                body.AddToClassList("et-detail-body");
                welcome.Add(body);
                details.Add(welcome);
                TraceLocalization.Apply(details);
                return;
            }
            AddEyebrow("EVENT DETAILS");
            var heading = new Label(record.EventName) { name = "trace-detail-title" };
            heading.AddToClassList("achieve-data");
            heading.AddToClassList("et-detail-title");
            details.Add(heading);
            var meta = new Label($"#{record.Id.Sequence}  ·  {record.Level}  ·  {record.Category}\n{record.ElapsedSeconds:0.000}s  /  frame {record.Frame}");
            meta.AddToClassList("et-detail-meta");
            details.Add(meta);
            AddEyebrow("RECORDED CAUSE");
            if (record.ParentId.IsValid && byId.TryGetValue(record.ParentId, out var parent))
                AddButton(details, $"↳ {parent.EventName}  ·  #{parent.Id.Sequence}", () => Reveal(parent.Id)).AddToClassList("achieve-data");
            else AddMuted(record.ParentId.IsValid ? "Parent belongs to another session or is outside this capture." : "Root event · no parent recorded.");
            var children = displayed.Records.Where(child => child.ParentId == record.Id).ToArray();
            if (children.Length > 0)
            {
                AddEyebrow($"RECORDED EFFECTS  ·  {children.Length}");
                foreach (var child in children.Take(20))
                {
                    var captured = child;
                    AddButton(details, $"{child.EventName}  ·  #{child.Id.Sequence}", () => Reveal(captured.Id)).AddToClassList("achieve-data");
                }
                if (children.Length > 20) AddMuted($"{children.Length - 20} more effects in the event list.");
            }
            AddEyebrow("PAYLOAD SNAPSHOT");
            AddCode(record.PayloadJson, "trace-payload");
            AddEyebrow("SOURCE");
            AddMuted(string.IsNullOrEmpty(record.SourceName) ? "No source object supplied." :
                $"{record.SourceName}  ·  object {record.SourceObjectId}");
            if (record.SourceObject != null)
                AddButton(details, "Select source object", () =>
                {
                    Selection.activeObject = record.SourceObject;
                    EditorGUIUtility.PingObject(record.SourceObject);
                });
            if (!string.IsNullOrEmpty(record.CallerFile))
            {
                AddEyebrow("CALL SITE");
                AddMuted($"{Path.GetFileName(record.CallerFile)}:{record.CallerLine}\n{record.CallerMember}");
                var open = AddButton(details, "Open code", () =>
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(record.CallerFile, record.CallerLine));
                open.SetEnabled(File.Exists(record.CallerFile));
                open.tooltip = record.CallerFile;
            }
            if (!string.IsNullOrEmpty(displayed.Session.MetadataJson) && displayed.Session.MetadataJson != "null")
            {
                AddEyebrow("SESSION METADATA");
                AddCode(displayed.Session.MetadataJson);
            }
            TraceLocalization.Apply(details);
        }

        private void AddEyebrow(string text)
        {
            var label = new Label(text);
            label.AddToClassList("et-eyebrow");
            details.Add(label);
        }

        private void AddMuted(string text)
        {
            var label = new Label(text);
            label.AddToClassList("et-detail-body");
            details.Add(label);
        }

        private void AddCode(string text, string name = null)
        {
            var field = new TextField { value = text, isReadOnly = true, multiline = true, name = name };
            field.AddToClassList("et-code");
            details.Add(field);
        }

        private void Reveal(TraceId id)
        {
            search.SetValueWithoutNotify(string.Empty);
            sourceSearch.SetValueWithoutNotify(string.Empty);
            category.SetValueWithoutNotify(category.choices[0]);
            level.SetValueWithoutNotify(level.choices[0]);
            selectedId = id;
            Refilter();
            var index = visible.FindIndex(record => record.Id == id);
            if (index >= 0) list.ScrollToItem(index);
        }

        private void TogglePause()
        {
            paused = !paused;
            if (!paused) lastVersion = -1;
            Refresh(true);
        }

        private void ShowLive()
        {
            imported = null;
            paused = false;
            lastVersion = -1;
            Refresh(true);
        }

        private void Export()
        {
            var path = EditorUtility.SaveFilePanel(TraceLocalization.Text("Export trace capture"), string.Empty, "event-trace", "etr");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                File.WriteAllText(path, (displayed ?? EventTrace.Capture()).ToJson());
                var assetPath = FileUtil.GetProjectRelativePath(path);
                if (assetPath.StartsWith("Assets/", StringComparison.Ordinal)) AssetDatabase.ImportAsset(assetPath);
            }
            catch (Exception exception) { ShowError(exception.Message); }
        }

        private void Import()
        {
            var path = EditorUtility.OpenFilePanelWithFilters(TraceLocalization.Text("Import trace capture"), string.Empty,
                new[] { "Trace captures", "etr,json" });
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                if (new FileInfo(path).Length > 16 * 1024 * 1024) throw new ArgumentException("Capture file exceeds 16 MB.");
                imported = TraceCapture.FromJson(File.ReadAllText(path));
                displayed = imported;
                selectedId = default;
                notice.style.display = DisplayStyle.None;
                Refresh(true);
            }
            catch (Exception exception) { ShowError(exception.Message); }
        }

        private void ShowError(string message)
        {
            notice.text = message;
            notice.style.display = DisplayStyle.Flex;
        }
    }
}
