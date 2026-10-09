using System;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Achieve.EventTracer.Editor
{
    [ScriptedImporter(1, "etr")]
    public sealed class TraceCaptureImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            try
            {
                if (new FileInfo(context.assetPath).Length > 16 * 1024 * 1024)
                    throw new ArgumentException("Capture file exceeds 16 MB.");
                var capture = TraceCapture.FromJson(File.ReadAllText(context.assetPath));
                var asset = ScriptableObject.CreateInstance<TraceCaptureAsset>();
                asset.name = Path.GetFileNameWithoutExtension(context.assetPath);
                asset.Initialize(capture);
                context.AddObjectToAsset("capture", asset);
                context.SetMainObject(asset);
            }
            catch (Exception exception)
            {
                context.LogImportError("Could not import trace capture: " + exception.Message);
                var error = new TextAsset(exception.Message) { name = "Invalid trace capture" };
                context.AddObjectToAsset("error", error);
                context.SetMainObject(error);
            }
        }
    }
}
