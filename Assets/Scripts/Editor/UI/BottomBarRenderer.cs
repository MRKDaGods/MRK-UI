using MRK.Utils;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MRK.UI
{
    public class BottomBarRenderer
    {
        private GUIStyle _style;
        private Layout[] _layoutArray;
        private string[] _layouts;
        private int _selectedLayoutIndex;
        private int _realLayoutIndex;
        private View[] _viewArray;
        private string[] _views;
        private int _selectedViewIndex;
        private int _realViewIndex;
        private bool _refreshForced = true;

        public void Draw(Rect windowRect)
        {
            if (_style == null)
            {
                _style = new GUIStyle
                {
                    normal =
                    {
                        background = UIUtility.GeneratePlainTexture(new Color(0.1f, 0.1f, 0.1f))
                    }
                };
            }

            if (_refreshForced || _layouts == null)
            {
                Reference<int> idx = new Reference<int>
                {
                    Value = 0
                };

                _layoutArray = new Layout[LayoutManager.Instance.Layouts.Count];
                _layouts = LayoutManager.Instance.Layouts.Select(layout =>
                {
                    _layoutArray[idx.Value++] = layout;
                    return layout.Identifier.FullIdentifier;
                }).ToArray();
            }

            GUILayout.BeginArea(windowRect);

            GUILayout.BeginHorizontal(_style);

            _selectedLayoutIndex = EditorGUILayout.Popup("Layout", _selectedLayoutIndex, _layouts);

            if (_refreshForced || _realLayoutIndex != _selectedLayoutIndex)
            {
                Reference<int> idx = new Reference<int>
                {
                    Value = 0
                };

                _realLayoutIndex = _selectedLayoutIndex;

                if (_layoutArray.Length > 0)
                {
                    _viewArray = new View[_layoutArray[_realLayoutIndex].Views.Count];
                    _views = _layoutArray[_realLayoutIndex].Views.Select(view =>
                    {
                        _viewArray[idx.Value++] = view;
                        return view.Name;
                    }).ToArray();
                }
            }

            if (_views != null)
            {
                _selectedViewIndex = EditorGUILayout.Popup("View", _selectedViewIndex, _views);
            }

            if (_refreshForced || _realViewIndex != _selectedViewIndex)
            {
                _realViewIndex = _selectedViewIndex;

                if (_viewArray != null && _viewArray.Length > 0)
                {
                    Debug.Log("Setting view to " + _viewArray[_realViewIndex].Name);
                    UIEditor.Instance.ViewRenderer.RenderedView = _viewArray[_realViewIndex];
                }
            }

            _refreshForced = false;
            if (GUILayout.Button("Refresh"))
            {
                _refreshForced = true;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}
