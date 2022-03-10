using UnityEditor;
using UnityEngine;

namespace MRK.UI
{
    public class CanvasSettingsRenderer : SettingsRenderer
    {
        private struct BuiltInProfile
        {
            public string Name;
            public float Width;
            public float Height;
            public float ScreenMatchMode;
        }

        private bool _foldout;

        private static readonly BuiltInProfile[] _builtInProfiles;

        static CanvasSettingsRenderer()
        {
            _builtInProfiles = new BuiltInProfile[1]
            {
                new BuiltInProfile
                {
                    Name = "Mobile Portrait",
                    Width = 1080f,
                    Height = 1980f,
                    ScreenMatchMode = 0.5f
                }
            };
        }

        public CanvasSettingsRenderer(UIEditor editor) : base(editor)
        {
        }

        public override void OnEnable()
        {
            CanvasSettings.Refresh();
        }

        private void InternalRender()
        {
            if (GUILayout.Button("Refresh"))
            {
                CanvasSettings.Refresh();
            }

            if (!CanvasSettings.CanvasDetected())
            {
                UIEditor.DrawLabel("<b>No canvases detected, please add one</b>");
                return;
            }

            GUILayout.Space(10f);

            CanvasSettings.Width = EditorGUILayout.FloatField("Width", CanvasSettings.Width);
            CanvasSettings.Height = EditorGUILayout.FloatField("Height", CanvasSettings.Height);
            CanvasSettings.ScreenMatchMode = EditorGUILayout.Slider("Screen Match Mode W/H", CanvasSettings.ScreenMatchMode, 0f, 1f);

            GUILayout.Space(10f);

            UIEditor.DrawLabel("<b>Profiles:</b>");

            foreach (BuiltInProfile profile in _builtInProfiles)
            {
                if (GUILayout.Button(profile.Name))
                {
                    CanvasSettings.Width = profile.Width;
                    CanvasSettings.Height = profile.Height;
                    CanvasSettings.ScreenMatchMode = profile.ScreenMatchMode;
                }
            }

            GUILayout.Space(10f);

            if (GUILayout.Button("Save"))
            {
                CanvasSettings.ApplyChanges();
            }
        }

        public override void Draw()
        {
            UIEditor.RenderVerticalFoldout(ref _foldout, "Canvas Settings", InternalRender);
        }
    }
}
