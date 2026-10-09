using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;

namespace Achieve.EventTracer.Editor
{
    [CustomEditor(typeof(TraceCaptureImporter))]
    public sealed class TraceCaptureImporterEditor : ScriptedImporterEditor
    {
        protected override bool needsApplyRevert => false;
        public override bool showImportedObject => false;
        public override VisualElement CreateInspectorGUI()
        {
            var path = ((TraceCaptureImporter)target).assetPath;
            var asset = AssetDatabase.LoadAssetAtPath<TraceCaptureAsset>(path);
            return TraceCaptureInspector.Build(asset);
        }
    }

    [CustomEditor(typeof(TraceCaptureAsset))]
    public sealed class TraceCaptureInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI() => Build((TraceCaptureAsset)target);

        internal static VisualElement Build(TraceCaptureAsset asset)
        {
            var root = new VisualElement { name = "trace-file-inspector" };
            root.style.paddingLeft = 4;
            root.style.paddingRight = 4;
            root.Add(TraceLocalization.CreateSelector());
            TraceLocalization.Install(root);
            if (asset == null || asset.Capture == null)
            {
                root.Add(new HelpBox("This .etr file could not be imported. Check its source JSON and Console error.", HelpBoxMessageType.Error));
                return root;
            }
            var capture = asset.Capture;
            var title = new Label(capture.Session.Name);
            title.AddToClassList("achieve-data");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginTop = 8;
            title.style.marginBottom = 8;
            root.Add(title);
            root.Add(new Label($"{capture.Records.Count:N0} events · {capture.DroppedCount:N0} dropped"));
            root.Add(new Label(capture.Session.StartedUtc));
            var open = new Button(() => EventTraceWindow.ShowCapture(capture))
                { text = "Open in Event Tracer", name = "open-trace-capture" };
            open.style.marginTop = 12;
            open.style.marginBottom = 12;
            root.Add(open);
            var metadata = new TextField("Session metadata")
                { value = capture.Session.MetadataJson, isReadOnly = true, multiline = true };
            root.Add(metadata);
            var preview = new Foldout { text = "Event preview", value = true };
            var count = Mathf.Min(20, capture.Records.Count);
            for (var i = 0; i < count; i++)
            {
                var record = capture.Records[i];
                var label = new Label($"{record.ElapsedSeconds:0.000}s  ·  {record.EventName}") { tooltip = record.PayloadJson };
                label.style.marginTop = 3;
                label.style.marginBottom = 3;
                preview.Add(label);
            }
            if (capture.Records.Count > count) preview.Add(new Label($"{capture.Records.Count - count:N0} more events in the trace window."));
            root.Add(preview);
            TraceLocalization.Apply(root);
            return root;
        }
    }
}
