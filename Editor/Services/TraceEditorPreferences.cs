using System;
using UnityEditor;
using UnityEngine;

namespace Achieve.EventTracer.Editor
{
    [InitializeOnLoad]
    internal static class TraceEditorPreferences
    {
        private const string Key = "Achieve.EventTracer.Configuration";
        static TraceEditorPreferences() => Apply();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            try
            {
                var json = EditorPrefs.GetString(Key, string.Empty);
                var configuration = new TraceConfiguration();
                // Keep defaults for fields absent from preferences saved by older versions.
                if (!string.IsNullOrEmpty(json)) JsonUtility.FromJsonOverwrite(json, configuration);
                EventTrace.Configure(configuration);
            }
            catch (Exception) { EventTrace.Configure(new TraceConfiguration()); }
        }

        internal static void Save(TraceConfiguration configuration)
        {
            EventTrace.Configure(configuration);
            EditorPrefs.SetString(Key, JsonUtility.ToJson(configuration));
        }
    }
}
