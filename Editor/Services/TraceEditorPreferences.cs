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
                EventTrace.Configure(string.IsNullOrEmpty(json)
                    ? new TraceConfiguration() : JsonUtility.FromJson<TraceConfiguration>(json));
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
