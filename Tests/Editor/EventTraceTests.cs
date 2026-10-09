using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Achieve.EventTracer.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;

namespace Achieve.EventTracer.Tests
{
    public sealed class EventTraceTests
    {
        private TraceConfiguration original;

        [SetUp] public void SetUp()
        {
            original = EventTrace.Configuration;
            EventTrace.Configure(new TraceConfiguration { maxEvents = 3 });
            EventTrace.StartSession("Test session");
        }

        [TearDown] public void TearDown()
        {
            EventTrace.Configure(original);
            EventTrace.StartSession("Editor session");
        }

        [Test] public void PayloadIsASnapshotOfFieldsAndAnonymousProperties()
        {
            var state = new State { health = 10 };
            EventTrace.Record("State", state);
            state.health = 99;
            EventTrace.Record("Shop.Buy", new { itemId = "sword", price = 100 });
            var records = EventTrace.GetSnapshot();
            Assert.That(records[0].PayloadJson, Does.Contain("\"health\":10"));
            Assert.That(records[1].PayloadJson, Does.Contain("\"itemId\":\"sword\""));
            Assert.That(records[1].Category, Is.EqualTo("Shop"));
        }

        [Test] public void RingBufferEvictsOldestAndCountsDrops()
        {
            for (var i = 0; i < 5; i++) EventTrace.Record("Event", i);
            var records = EventTrace.GetSnapshot();
            Assert.That(records, Has.Length.EqualTo(3));
            Assert.That(records[0].Id.Sequence, Is.EqualTo(3));
            Assert.That(EventTrace.DroppedCount, Is.EqualTo(2));
        }

        [Test] public void CharacterBudgetIsEnforcedIndependentlyOfCount()
        {
            EventTrace.Configure(new TraceConfiguration { maxEvents = 100, maxPayloadCharacters = 128, maxBufferCharacters = 256 });
            for (var i = 0; i < 4; i++) EventTrace.Record("Event", new { value = i }, callerFile: "", callerMember: "");
            Assert.That(EventTrace.BufferedCharacters, Is.LessThanOrEqualTo(256));
            Assert.That(EventTrace.DroppedCount, Is.GreaterThan(0));
        }

        [Test] public void ResizePreservesNewestRecords()
        {
            for (var i = 0; i < 3; i++) EventTrace.Record("Event", i);
            EventTrace.Configure(new TraceConfiguration { maxEvents = 2 });
            Assert.That(EventTrace.GetSnapshot()[0].Id.Sequence, Is.EqualTo(2));
            Assert.That(EventTrace.DroppedCount, Is.EqualTo(1));
        }

        [Test] public void ExplicitParentSurvivesExportAndImport()
        {
            var parent = EventTrace.Record("Shop.Buy");
            EventTrace.Record("Wallet.Spend", new { before = 500, after = 400 }, parentId: parent);
            var capture = TraceCapture.FromJson(EventTrace.Capture().ToJson());
            Assert.That(capture.Records[1].ParentId, Is.EqualTo(parent));
            Assert.That(capture.Records[1].PayloadJson, Does.Contain("\"after\":400"));
            Assert.That(capture.Session.Name, Is.EqualTo("Test session"));
        }

        [Test] public void ClearDoesNotReuseSequenceIds()
        {
            var first = EventTrace.Record("First");
            EventTrace.Clear();
            var second = EventTrace.Record("Second");
            Assert.That(second.Sequence, Is.GreaterThan(first.Sequence));
        }

        [Test] public void ParentFromPreviousSessionDoesNotLinkToReusedSequence()
        {
            var old = EventTrace.Record("Old root");
            EventTrace.StartSession("New session");
            var current = EventTrace.Record("New root");
            EventTrace.Record("Child", parentId: old);
            var child = EventTrace.GetSnapshot()[1];
            Assert.That(child.ParentId, Is.EqualTo(old));
            Assert.That(child.ParentId, Is.Not.EqualTo(current));
        }

        [Test] public void DisabledRecorderDoesNotReadPayload()
        {
            var configuration = EventTrace.Configuration;
            configuration.enabled = false;
            EventTrace.Configure(configuration);
            EventTrace.Record("Disabled", new ThrowingGetter());
            Assert.That(EventTrace.Count, Is.Zero);
        }

        [Test] public void CyclesAndThrowingGettersAreRepresentedWithoutCrashing()
        {
            var cycle = new Cycle();
            cycle.self = cycle;
            EventTrace.Record("Cycle", cycle);
            EventTrace.Record("Getter", new ThrowingGetter());
            Assert.That(EventTrace.GetSnapshot()[0].PayloadJson, Does.Contain("<cycle>"));
            Assert.That(EventTrace.GetSnapshot()[1].PayloadJson, Does.Contain("unreadable"));
        }

        [Test] public void OversizedPayloadProducesValidTruncationMarker()
        {
            EventTrace.Configure(new TraceConfiguration { maxPayloadCharacters = 128 });
            EventTrace.Record("Large", new { text = new string('x', 10000) });
            Assert.That(EventTrace.GetSnapshot()[0].PayloadJson, Is.EqualTo("{\"$truncated\":true}"));
        }

        [Test] public void JsonStringsEscapeQuotesControlCharactersAndUnicode()
        {
            EventTrace.Record("Escaping", new { text = "a\"b\\c\n\t😀" });
            var json = EventTrace.GetSnapshot()[0].PayloadJson;
            var parsed = JsonUtility.FromJson<TextState>(json);
            Assert.That(parsed.text, Is.EqualTo("a\"b\\c\n\t😀"));
        }

        [Test] public void NumberFormattingUsesInvariantCulture()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                EventTrace.Record("Number", new { value = 1.25 });
                Assert.That(EventTrace.GetSnapshot()[0].PayloadJson, Does.Contain("1.25"));
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Test] public void InvalidConfigurationDoesNotChangeRecorder()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => EventTrace.Configure(new TraceConfiguration { maxEvents = 0 }));
            Assert.That(EventTrace.Configuration.maxEvents, Is.EqualTo(3));
        }

        [Test] public void ImportDoesNotReplaceLiveSession()
        {
            EventTrace.Record("Saved");
            var json = EventTrace.Capture().ToJson();
            var current = EventTrace.StartSession("Live");
            TraceCapture.FromJson(json);
            Assert.That(EventTrace.Session.Id, Is.EqualTo(current.Id));
            Assert.That(EventTrace.Count, Is.Zero);
        }

        [TestCase("{}")]
        [TestCase("{\"formatVersion\":99}")]
        [TestCase("bad json")]
        public void ImportRejectsInvalidFiles(string json) => Assert.Throws<ArgumentException>(() => TraceCapture.FromJson(json));

        [Test] public void MainThreadContractIsEnforced()
        {
            var error = Task.Run(() =>
            {
                try { EventTrace.Record("Worker"); return (Exception)null; }
                catch (Exception exception) { return exception; }
            }).GetAwaiter().GetResult();
            Assert.That(error, Is.TypeOf<InvalidOperationException>());
        }

        [Test] public void CallerLocationIsCaptured()
        {
            EventTrace.Record("Caller");
            var record = EventTrace.GetSnapshot()[0];
            Assert.That(record.CallerFile, Does.EndWith("EventTraceTests.cs"));
            Assert.That(record.CallerMember, Is.EqualTo(nameof(CallerLocationIsCaptured)));
            Assert.That(record.CallerLine, Is.GreaterThan(0));
        }

        [Test] public void WindowBuildsToolkitBrowserAndFiltersActualRecords()
        {
            EventTrace.Record("Shop.Buy");
            EventTrace.Record("Quest.Start");
            var window = ScriptableObject.CreateInstance<EventTraceWindow>();
            try
            {
                window.CreateGUI();
                var list = window.rootVisualElement.Q<ListView>("trace-event-list");
                Assert.That(list.itemsSource.Count, Is.EqualTo(2));
                window.SetSearchFilter("Shop");
                Assert.That(list.itemsSource.Count, Is.EqualTo(1));
                Assert.That(window.rootVisualElement.Query<IMGUIContainer>().ToList(), Is.Empty);
                Assert.That(window.rootVisualElement.styleSheets.count, Is.GreaterThan(0));
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }

        [Serializable] private sealed class State { public int health; }

        [Test] public void LanguageSwitchPreservesEventNamesAndFilterValues()
        {
            var existed = EditorPrefs.HasKey("Achieve.Editor.Language");
            var previous = EditorPrefs.GetString("Achieve.Editor.Language");
            var window = ScriptableObject.CreateInstance<EventTraceWindow>();
            try
            {
                TraceLocalization.SetLanguage(false);
                EventTrace.Record("Clear", new { text = "Export" });
                window.CreateGUI();
                window.SetSearchFilter("Clear");
                TraceLocalization.SetLanguage(true);
                TraceLocalization.Apply(window.rootVisualElement);
                Assert.That(window.rootVisualElement.Q<Button>("trace-export").text, Is.EqualTo("내보내기"));
                Assert.That(window.rootVisualElement.Q<ToolbarSearchField>("trace-search").value, Is.EqualTo("Clear"));
                Assert.That(window.rootVisualElement.Q<ListView>("trace-event-list").itemsSource.Count, Is.EqualTo(1));
                Assert.That(EventTrace.GetSnapshot()[0].EventName, Is.EqualTo("Clear"));
                TraceLocalization.SetLanguage(false);
                TraceLocalization.Apply(window.rootVisualElement);
                Assert.That(window.rootVisualElement.Q<Button>("trace-export").text, Is.EqualTo("Export"));
                var summary = new Label();
                TraceLocalization.SetLanguage(true);
                TraceLocalization.SetText(summary, "2 shown · 3 captured · 1 dropped");
                Assert.That(summary.text, Is.EqualTo("표시 2 · 수집 3 · 제외 1"));
                TraceLocalization.SetLanguage(false);
                TraceLocalization.Apply(summary);
                Assert.That(summary.text, Is.EqualTo("2 shown · 3 captured · 1 dropped"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(window);
                if (existed) EditorPrefs.SetString("Achieve.Editor.Language", previous);
                else EditorPrefs.DeleteKey("Achieve.Editor.Language");
            }
        }
        [Serializable] private sealed class TextState { public string text; }
        private sealed class Cycle { public Cycle self; }
        private sealed class ThrowingGetter { public string Value => throw new InvalidOperationException("bad getter"); }
    }
}
