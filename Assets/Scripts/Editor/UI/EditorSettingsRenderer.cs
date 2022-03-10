using UnityEditor;

namespace MRK.UI
{
    public class EditorSettingsRenderer : SettingsRenderer
    {
        private bool _foldout;

        public EditorSettingsRenderer(UIEditor editor) : base(editor)
        {
        }

        private void InternalRender()
        {
            EditorSettings.ViewRendererZoom = EditorGUILayout.Slider("View Renderer zoom", EditorSettings.ViewRendererZoom, 0.2f, 2f);
        }

        public override void Draw()
        {
            UIEditor.RenderVerticalFoldout(ref _foldout, "Editor Settings", InternalRender);
        }
    }
}
