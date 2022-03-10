using MRK.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MRK.UI
{
    public partial class LayoutSettingsRenderer : SettingsRenderer
    {
        private const string FoldoutKey = "foldout";

        private bool _initializedStyles;
        private GUIStyle _verticalStyle;
        private readonly HashSet<View> _viewDeletionQueue;
        private bool _showCreateViewDialog;
        private string _createViewName;

        private LayoutManager LayoutManager
        {
            get { return LayoutManager.Instance; }
        }

        public LayoutSettingsRenderer(UIEditor editor) : base(editor)
        {
            _viewDeletionQueue = new HashSet<View>();
            _graphicRemovalQueue = new HashSet<GraphicElement>();
        }

        public override void OnEnable()
        {
            LayoutManager.Instance.Initialize(false);
        }

        private void RenderLayout(Layout layout)
        {
            layout.Layer = EditorGUILayout.IntSlider("Layer", layout.Layer, 0, ContainerManager.LayerMax - 1);
            GUILayout.Space(5f);

            var so = (SerializedObject)layout.EditorStorage["so0"];
            EditorGUILayout.PropertyField(so.FindProperty("_registry"));

            bool viewFoldout = (bool)layout.EditorStorage[$"{FoldoutKey}V"];
            UIEditor.RenderVerticalFoldoutNested(ref viewFoldout, "Views", () => RenderViewList(layout, layout.Views));
            layout.EditorStorage[$"{FoldoutKey}V"] = viewFoldout;

            so.ApplyModifiedProperties();
        }

        private void RenderViewList(Layout layout, List<View> views)
        {
            if (layout == null || views == null || views.Count == 0)
            {
                GUILayout.Label("No views to render");
            }
            else
            {
                _viewDeletionQueue.Clear();

                try
                {
                    foreach (View view in views)
                    {
                        GUILayout.BeginVertical(_graphicStyle);

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button($"<b>{view.Name}</b>", _graphicStyle))
                        {
                            Selection.activeGameObject = view.ViewRoot.gameObject;
                            SceneSettings.Focus(view.ViewRoot.position);
                        }

                        if (GUILayout.Button("^", GUILayout.Width(50f)))
                        {
                            view.ViewIndex--;
                        }

                        if (GUILayout.Button("v", GUILayout.Width(50f)))
                        {
                            view.ViewIndex++;
                        }

                        var oldEditorFoldout = view.EditorFoldout;
                        if (GUILayout.Button(view.EditorFoldout ? "-" : "+", GUILayout.Width(50f)))
                        {
                            view.EditorFoldout = !view.EditorFoldout;
                        }

                        var oldCol = GUI.color;
                        GUI.color = Color.red;
                        if (GUILayout.Button("X", GUILayout.Width(50f)))
                        {
                            _viewDeletionQueue.Add(view);
                        }
                        GUI.color = oldCol;

                        GUILayout.EndHorizontal();

                        if (view.EditorFoldout)
                        {
                            //always init view upon expansion
                            if (oldEditorFoldout != view.EditorFoldout)
                            {
                                view.Initialize(layout);
                            }

                            view.Name = EditorGUILayout.TextField("Name", view.Name);
                            view.SafeRender = EditorGUILayout.Toggle("Safe Render", view.SafeRender);
                            view.LayoutGroupType = (LayoutGroupType)EditorGUILayout.EnumPopup("Layout Group", view.LayoutGroupType);

                            UIEditor.RenderVerticalFoldoutNested(ref view.EditorScrollFoldout, "Scroll Settings", () => RenderScrollSettings(view));

                            GUILayout.Space(5f);
                            if (GUILayout.Button("Refresh Indices"))
                            {
                                view.RefreshGraphicIndices();
                            }

                            UIEditor.RenderVerticalFoldoutNested(ref view.EditorGraphicsFoldout, "Graphics", () => RenderViewGraphics(view));
                        }

                        GUILayout.EndVertical();
                    }
                }
                catch (InvalidOperationException)
                {
                }

                _viewDeletionQueue.RemoveWhere(view =>
                {
                    layout.RemoveView(view);
                    return true;
                });
            }


            if (GUILayout.Button("Create"))
            {
                _showCreateViewDialog = !_showCreateViewDialog;
                if (_showCreateViewDialog)
                {
                    _createViewName = string.Empty;
                }
            }

            if (_showCreateViewDialog)
            {
                _createViewName = EditorGUILayout.TextField("Name", _createViewName);

                if (GUILayout.Button("Add"))
                {
                    _showCreateViewDialog = false;
                    Debug.Log($"Creating view {_createViewName} res={LayoutManager.CreateView(layout, _createViewName)}");
                }
            }
        }

        private void CheckStyles()
        {
            if (!_initializedStyles)
            {
                _verticalStyle = new GUIStyle
                {
                    normal =
                    {
                        background = UIUtility.GeneratePlainTexture(new Color(0.2f, 0.2f, 0.2f))
                    },
                    padding = new RectOffset(3, 3, 5, 5),
                    richText = true
                };

                _graphicStyle = new GUIStyle(GUI.skin.box)
                {
                    normal =
                    {
                        textColor = Color.white
                    },
                    richText = true,
                    stretchWidth = true
                };

                _graphicAddStyle = new GUIStyle(GUI.skin.button)
                {
                    richText = true
                };

                _initializedStyles = true;
            }
        }

        private void RenderLayoutList()
        {
            foreach (var layout in LayoutManager.Layouts)
            {
                UIEditor.DrawLabel($"<b>{layout.LayoutType}</b>");
                
                bool foldout = (bool)layout.EditorStorage[FoldoutKey];
                UIEditor.RenderVerticalFoldoutNested(ref foldout, layout.Identifier.ContextualIdentifier, () => RenderLayout(layout));
                layout.EditorStorage[FoldoutKey] = foldout;
            }
        }

        public override void Draw()
        {
            CheckStyles();

            GUILayout.Space(10f);
            GUILayout.BeginVertical(_verticalStyle);
            UIEditor.DrawLabel("<b>Layouts</b>");
            GUILayout.Space(5f);
            RenderLayoutList();
            GUILayout.EndVertical();
        }

        private void RenderScrollSettings(View view)
        {
            var ss = view.ScrollSettings;
            ss.Scrollable = EditorGUILayout.Toggle("Scrollable", ss.Scrollable);
            if (ss.Scrollable)
            {
                ss.ScrollVertical = EditorGUILayout.Toggle("Vertical", ss.ScrollVertical);
                ss.ScrollHorizontal = EditorGUILayout.Toggle("Horizontal", ss.ScrollHorizontal);
            }
        }
    }
}
