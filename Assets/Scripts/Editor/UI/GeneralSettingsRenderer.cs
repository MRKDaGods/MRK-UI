using UnityEditor;
using UnityEngine;
using System.Linq;

namespace MRK.UI
{
    public class GeneralSettingsRenderer : SettingsRenderer
    {
        private bool _screenSettingsFoldout;
        private string[] _screenIdentifiers;
        private int _selectedScreenIdentifierIndex;

        public GeneralSettingsRenderer(UIEditor editor) : base(editor)
        {
        }

        private void RenderScreenSettings()
        {
            if (_screenIdentifiers == null)
            {
                _screenIdentifiers = LayoutIdentifier.ScreenIdentifiers.Select(x => x.FullIdentifier).ToArray();
            }

            _selectedScreenIdentifierIndex = EditorGUILayout.Popup("Screen Type", _selectedScreenIdentifierIndex, _screenIdentifiers);
            if (GUILayout.Button("Create screen"))
            {
                if (!LayoutManager.Instance.CreateScreen(_screenIdentifiers[_selectedScreenIdentifierIndex]))
                {
                    Debug.LogError("Cannot create screen");
                }
            }
        }

        public override void Draw()
        {
            UIEditor.RenderVerticalFoldout(ref _screenSettingsFoldout, "Screen Settings", RenderScreenSettings);
        }
    }
}
