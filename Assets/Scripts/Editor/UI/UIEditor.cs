using MRK.Utils;
using System;
using UnityEditor;
using UnityEngine;

namespace MRK.UI
{
    public class UIEditor : EditorWindow
    {
        private const float BottomBarHeight = 50f;

        private bool _initializedStyles;
        private GUIStyle _leftBarStyle;
        private GUIStyle _contextualBlockStyle;
        private GUIStyle _labelStyle;
        private readonly SettingsRenderer[] _settingsRenderers;
        private readonly ViewRenderer _viewRenderer;
        private readonly BottomBarRenderer _bottomBarRenderer;
        private Vector2 _scrollDelta;
        private readonly UICamera _uiCamera;
        private float _lastRepaintTime;

        private static UIEditor _instance;

        public GUIStyle ContextualBlockStyle
        {
            get { return _contextualBlockStyle; }
        }

        public ViewRenderer ViewRenderer
        {
            get { return _viewRenderer; }
        }

        public static UIEditor Instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = GetWindow<UIEditor>();
                }

                return _instance;
            }
        }

        public UIEditor()
        {
            _settingsRenderers = new SettingsRenderer[]
            {
                new EditorSettingsRenderer(this),
                new CanvasSettingsRenderer(this),
                new GeneralSettingsRenderer(this),
                new LayoutSettingsRenderer(this)
            };

            _uiCamera = new UICamera();
            _viewRenderer = new ViewRenderer(_uiCamera);
            _bottomBarRenderer = new BottomBarRenderer();
        }

        [MenuItem("MRK/UI/Editor")]
        private static void OnContextMenuClick()
        {
            Instance.Show();
            //Instance.wantsMouseMove = true;
        }

        [MenuItem("MRK/Unlock Assemblies")]
        private static void OnUnlockAssembliesClick()
        {
            EditorApplication.UnlockReloadAssemblies();
        }

        private void OnEnable()
        {
            foreach (var renderer in _settingsRenderers)
            {
                renderer.OnEnable();
            }

            _uiCamera.Setup();
            CanvasSettings.UICamera = _uiCamera.Camera;
        }

        private void Update()
        {
            float interval = 1f / 60f; //60fps
            if (Time.time - _lastRepaintTime >= interval)
            {
                _lastRepaintTime = Time.time;
                Repaint();
            }
        }

        private void OnDisable()
        {
            //_uiCamera.Clean();
        }

        private void OnValidate()
        {
            //this prevents unwanted runtime errors
            //Instance.Close();
        }

        private void CheckStyles()
        {
            if (!_initializedStyles)
            {
                _leftBarStyle = new GUIStyle
                {
                    normal =
                    {
                        background = UIUtility.GeneratePlainTexture(new Color(0.3f, 0.3f, 0.3f))
                    }
                };


                _contextualBlockStyle = new GUIStyle
                {
                    normal =
                    {
                        background = UIUtility.GeneratePlainTexture(new Color32(56, 56, 56, 255))
                    },

                    padding = new RectOffset(10, 10, 10, 10)
                };

                _labelStyle = new GUIStyle()
                {
                    normal =
                    {
                        textColor = Color.white
                    },
                    padding = new RectOffset(5, 0, 5, 0),
                    richText = true
                };

                _initializedStyles = true;
            }
        }

        public void DrawLabel(string label)
        {
            GUILayout.Label(label, _labelStyle);
        }

        private void OnGUI()
        {
            CheckStyles();

            Rect leftBarRect = new Rect(0f, 0f, /*Mathf.Min(position.width / 2.5f, 500f)*/position.width, position.height);
            GUILayout.BeginArea(leftBarRect, _leftBarStyle);

            DrawLabel("<b>MRK UI Editor alpha</b>");
            GUILayout.Space(5f);

            _scrollDelta = GUILayout.BeginScrollView(_scrollDelta);

            foreach (var renderer in _settingsRenderers)
            {
                renderer.Draw();
            }

            if (SceneSettings.Editing)
            {
                var oldColor = GUI.color;
                GUI.color = Color.red;

                if (GUILayout.Button("Exit edit mode"))
                {
                    SceneSettings.EndEdit();

                    OnUnlockAssembliesClick();
                }

                GUI.color = oldColor;
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            /*Rect viewRect = new Rect(leftBarRect.width, 0f, position.width - leftBarRect.width, position.height);
            _viewRenderer.Draw(viewRect);

            Rect bottomRect = new Rect(viewRect.x, position.height - BottomBarHeight * 0.5f, viewRect.width, BottomBarHeight);
            _bottomBarRenderer.Draw(bottomRect); */
        }

        public void RenderVerticalFoldout(ref bool foldout, string label, Action render)
        {
            if (render == null) return;

            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);
            {
                if (foldout)
                {
                    GUILayout.BeginVertical(_contextualBlockStyle);
                    render();
                    GUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void RenderVerticalFoldoutNested(ref bool foldout, string label, Action render, GUIStyle style = null)
        {
            if (render == null) return;

            foldout = EditorGUILayout.Foldout(foldout, label);
            {
                if (foldout)
                {
                    GUILayout.BeginVertical(style ?? _contextualBlockStyle);
                    render();
                    GUILayout.EndVertical();
                }
            }
        }

        public void SetActiveGraphic(GraphicElement graphic)
        {
            if (graphic == null) return;

            //enable just our layer, now disable all other layouts
            ContainerManager.Instance.EnableLayerSingular(graphic.View.Layout.Layer, out LayoutLayer layer);
            layer.EnableLayoutSingular(graphic.View.Layout);
        }

        public void OnExitEditMode()
        {
            Selection.activeGameObject = null;
            ContainerManager.Instance.EnableAllLayers(true);
        }
    }
}
