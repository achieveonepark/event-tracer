using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Achieve.EventTracer.Samples.Editor
{
    public static class ShopTraceDemoSetup
    {
        [MenuItem("Tools/Achieve/Samples/Create Shop Trace Demo")]
        public static void CreateDemo()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var folder = AssetDatabase.GenerateUniqueAssetPath("Assets/EventTracerDemo");
            AssetDatabase.CreateFolder("Assets", System.IO.Path.GetFileName(folder));
            CreateDemoAt(folder);
            Achieve.EventTracer.Editor.EventTraceWindow.Open();
        }

        public static void CreateDemoAt(string folder)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.13f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            new GameObject("Shop demo").AddComponent<ShopTraceDemo>();
            EditorSceneManager.SaveScene(scene, folder + "/ShopDemo.unity");
            AssetDatabase.SaveAssets();
        }
    }
}
