using System;
using System.IO;
using Achieve.EventTracer.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Achieve.EventTracer.Tests
{
    public sealed class TraceCaptureFileTests
    {
        [Test] public void EtrImportsAsCaptureWithToolkitInspectorAndKeepsLiveSession()
        {
            var folder = "Assets/TraceCaptureTest_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
            EventTrace.StartSession("Saved capture");
            var rootEvent = EventTrace.Record("Shop.Buy");
            EventTrace.Record("Wallet.Spend", new { amount = 100 }, parentId: rootEvent);
            var path = folder + "/Session.etr";
            File.WriteAllText(path, EventTrace.Capture().ToJson());
            var live = EventTrace.StartSession("Live session");
            UnityEditor.Editor editor = null;
            try
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var asset = AssetDatabase.LoadAssetAtPath<TraceCaptureAsset>(path);
                Assert.That(asset, Is.Not.Null);
                Assert.That(asset.Capture.Records.Count, Is.EqualTo(2));
                Assert.That(asset.Capture.Records[1].ParentId, Is.EqualTo(rootEvent));
                Assert.That(EventTrace.Session.Id, Is.EqualTo(live.Id));
                editor = UnityEditor.Editor.CreateEditor(AssetImporter.GetAtPath(path));
                Assert.That(editor, Is.TypeOf<TraceCaptureImporterEditor>());
                var ui = editor.CreateInspectorGUI();
                Assert.That(ui.Query<IMGUIContainer>().ToList(), Is.Empty);
                Assert.That(ui.Query<Button>().ToList(), Is.Not.Empty);
            }
            finally
            {
                if (editor != null) UnityEngine.Object.DestroyImmediate(editor);
                AssetDatabase.DeleteAsset(folder);
                EventTrace.StartSession("Editor session");
            }
        }
    }
}
