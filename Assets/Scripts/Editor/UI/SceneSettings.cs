using UnityEditor;
using UnityEngine;

namespace MRK.UI
{
    public class SceneSettings
    {
        private class Settings
        {
            public bool In2DMode;
            public Material Skybox;
        }

        private static SceneView _sceneView;
        private static bool _editing;
        private static Settings _settings;

        public static bool Editing
        {
            get { return _editing; }
        }

        public static void BeginEdit()
        {
            if (_editing) return;

            _sceneView = SceneView.lastActiveSceneView;
            if (_sceneView == null)
            {
                Debug.LogError("Cannot find scene view");
                return;
            }

            if (_settings == null)
            {
                _settings = new Settings();
            }

            _settings.In2DMode = _sceneView.in2DMode;
            _sceneView.in2DMode = true;

            _settings.Skybox = RenderSettings.skybox;
            RenderSettings.skybox = null;

            _editing = true;

            EditorApplication.LockReloadAssemblies();
        }

        public static void Focus(Vector3 pos)
        {
            if (!_editing) return;

            _sceneView.LookAt(pos);
        }

        public static void EndEdit()
        {
            if (!_editing) return;

            _sceneView.in2DMode = _settings.In2DMode;
            RenderSettings.skybox = _settings.Skybox;

            UIEditor.Instance.OnExitEditMode();

            _editing = false;
        }
    }
}
