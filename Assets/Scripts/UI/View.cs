using MRK.UI.Animation;
using MRK.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MRK.UI
{
    [Serializable]
    public class View : IUIAnimatable, IUIAnimatableAlpha, IUIAnimatableMove
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private bool _visible = true;
        [SerializeField]
        private Transform _viewRoot;
        [SerializeField]
        private List<GraphicElement> _graphics;
        [SerializeField]
        private List<GraphicGroup> _graphicGroups;
        [SerializeField]
        private bool _safeRender;
        [SerializeField]
        private LayoutGroupType _layoutGroupType;
        [SerializeField]
        private LayoutGroup _layoutGroup;
        [SerializeField]
        private ScrollSettings _scrollSettings;
        private Layout _layout;
        private CachedRectTransform _cachedRectTransform;
        private int? _viewIndex;
        private CanvasGroup _canvasGroup;
        private readonly UIAnimator _animator;
        private RectTransform _content;
        private ScrollRect _scrollRect;
        private Scrollbar _scrollVertical;
        private Scrollbar _scrollHorizontal;
        [SerializeField, HideInInspector]
        private Vector2 _oldContentSize;

        private static GameObject _viewPrefab;

#if UNITY_EDITOR
        [HideInInspector]
        public bool EditorFoldout;
        [HideInInspector]
        public bool EditorGraphicsFoldout;
        [HideInInspector]
        public bool EditorScrollFoldout;
#endif

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;

                    if (_viewRoot != null)
                    {
                        _viewRoot.name = _name;
                    }
                }
            }
        }

        public Transform ViewRoot
        {
            get { return _viewRoot; }
            set { _viewRoot = value; }
        }

        public Transform ContentRoot
        {
            get { return _content; /*ScrollSettings.Scrollable ? _content : _viewRoot;*/ }
        }

        public List<GraphicElement> Graphics
        {
            get { return _graphics; }
        }

        public Layout Layout
        {
            get
            {
                if (_layout == null)
                {
                    _layout = _viewRoot.parent.GetComponent<Layout>();
                }

                return _layout;
            }
        }

        public bool SafeRender
        {
            get { return _safeRender; }
            set
            {
                if (_safeRender != value)
                {
                    _safeRender = value;
                    UpdateViewAnchors();
                }
            }
        }

        public LayoutGroupType LayoutGroupType
        {
            get { return _layoutGroupType; }
            set { SetLayoutGroupType(value); }
        }

        public LayoutGroup LayoutGroup
        {
            get { return _layoutGroup; }
        }

        public ScrollSettings ScrollSettings
        {
            get
            {
                if (_scrollSettings == null)
                {
                    _scrollSettings = new ScrollSettings();
                }

                return _scrollSettings;
            }
        }

        public int ViewIndex
        {
            get { return _viewIndex ?? _viewRoot.GetSiblingIndex(); }
            set
            {
                if (!_viewIndex.HasValue || _viewIndex.Value != value)
                {
                    SetViewIndex(value);
                }
            }
        }

        public bool Visible
        {
            get { return _visible; }
        }

        public Vector2 AnchorMin
        {
            get { return _cachedRectTransform.RectTransform.anchorMin; }
            set { _cachedRectTransform.RectTransform.anchorMin = value; }
        }

        public Vector2 AnchorMax
        {
            get { return _cachedRectTransform.RectTransform.anchorMax; }
            set { _cachedRectTransform.RectTransform.anchorMax = value; }
        }

        public UIAnimator Animator
        {
            get { return _animator; }
        }

        public View()
        {
            _animator = new UIAnimator(this);
        }

        public void Initialize(Layout layout)
        {
            if (layout == null) return;

            if (_viewRoot == null)
            {
                if (_viewPrefab == null)
                {
                    _viewPrefab = Resources.Load<GameObject>("UI/Templates/Internal/View");
                }

                var viewObj = UnityEngine.Object.Instantiate(_viewPrefab);
                viewObj.name = _name;
                viewObj.layer = ContainerManager.UIOnlyLayer;
                viewObj.transform.parent = layout.transform;
                _viewRoot = TransformUtility.SwitchToRectTransform(viewObj.transform);

                TransformUtility.ResetLocals(_viewRoot);
                TransformUtility.OverrideAnchorsStretched((RectTransform)_viewRoot);

                _viewRoot = viewObj.transform;
            }

            if (_graphics == null)
            {
                _graphics = new List<GraphicElement>();
            }

            if (_graphicGroups == null)
            {
                _graphicGroups = new List<GraphicGroup>();
            }

            _cachedRectTransform = new CachedRectTransform(_viewRoot.gameObject);

            //safe render?
            UpdateViewAnchors();

            //force visible state
            SetVisible(_visible);

            ScrollSettings.OnScrollChanged = OnScrollStateChanged;

            if (_content == null)
            {
                _content = (RectTransform)_viewRoot.Find("Viewport/Content");
            }

            if (_scrollRect == null)
            {
                _scrollRect = _viewRoot.GetComponent<ScrollRect>();
                if (_scrollRect == null)
                {
                    Debug.LogError($"Scroll rect is not available view={Name}");
                }
            }

            if (_scrollVertical == null)
            {
                _scrollVertical = _viewRoot.Find("Scrollbar Vertical")?.GetComponent<Scrollbar>();
            }

            if (_scrollHorizontal == null)
            {
                _scrollHorizontal = _viewRoot.Find("Scrollbar Horizontal")?.GetComponent<Scrollbar>();
            }
        }

        public void Destroy()
        {
            if (_viewRoot != null)
            {
                ObjectUtility.SafeDestroy(_viewRoot.gameObject);
            }
        }

        public void RemoveGraphic(GraphicElement graphic)
        {
            if (!_graphics.Remove(graphic)) return;

            ObjectUtility.SafeDestroy(graphic.Graphic.gameObject);
        }

        public void SetLayoutGroupType(LayoutGroupType type)
        {
            if (_layoutGroupType == type) return;

            if (_layoutGroup != null)
            {
                UnityEngine.Object.DestroyImmediate(_layoutGroup);
            }

            _layoutGroupType = type;

            switch (type)
            {
                case LayoutGroupType.Vertical:
                    _layoutGroup = ContentRoot.gameObject.AddComponent<VerticalLayoutGroup>();
                    break;

                case LayoutGroupType.Horizontal:
                    _layoutGroup = ContentRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
                    break;

                case LayoutGroupType.Grid:
                    _layoutGroup = ContentRoot.gameObject.AddComponent<GridLayoutGroup>();
                    break;
            }
        }

        public void Show()
        {
            if ((_visible && !_animator.IsAnimatingHide) || _animator.IsAnimatingShow) return;

            SetVisible(true);
        }

        public void Show(UIAnimation animation, params object[] contextArgs)
        {
            if ((_visible && !_animator.IsAnimatingHide) || _animator.IsAnimatingShow) return;

            SetVisible(true, false);
            _animator.PlayAnimation(animation, UIAnimationMode.Show, contextArgs);
        }

        public void Hide()
        {
            if ((!_visible && !_animator.IsAnimatingShow) || _animator.IsAnimatingHide) return;

            SetVisible(false);
        }

        public void Hide(UIAnimation animation, params object[] contextArgs)
        {
            if ((!_visible && !_animator.IsAnimatingShow) || _animator.IsAnimatingHide) return;

            _animator.PlayAnimation(animation, UIAnimationMode.Hide, contextArgs);
        }

        public void SetVisible(bool visible, bool updateLocalState = true)
        {
            if (updateLocalState)
            {
                _visible = visible;
            }

            _viewRoot.gameObject.SetActive(visible);
        }

        private void UpdateViewAnchors()
        {
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;

            if (_safeRender)
            {
                Rect safeArea = Screen.safeArea;
                anchorMin = safeArea.position;
                anchorMax = safeArea.position + safeArea.size;
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;
            }

            _cachedRectTransform.RectTransform.anchorMin = anchorMin;
            _cachedRectTransform.RectTransform.anchorMax = anchorMax;
        }

        private void SetViewIndex(int index)
        {
            if (Layout.Views.Count <= index || index < 0) return;

            var tmp = Layout.Views[index];
            Layout.Views[index] = this;
            Layout.Views[ViewIndex] = tmp;

            _viewIndex = index;
            _viewRoot.SetSiblingIndex(index);
        }

        public void SetRTL(bool rtl)
        {
            if (_layoutGroup == null) return;

            switch (_layoutGroupType)
            {
                case LayoutGroupType.Vertical:
                    ((VerticalLayoutGroup)_layoutGroup).reverseArrangement = rtl;
                    break;

                case LayoutGroupType.Horizontal:
                    ((HorizontalLayoutGroup)_layoutGroup).reverseArrangement = rtl;
                    break;
            }
        }

        public GraphicGroup GetNewGraphicGroup(GraphicElement graphicElement)
        {
            var graphicGroup = new GraphicGroup();
            graphicGroup.Initialize(graphicElement);

            _graphicGroups.Add(graphicGroup);

            return graphicGroup;
        }

        public GraphicGroup GetGraphicGroup(int groupId)
        {
            return groupId == -1 ? null : _graphicGroups.Find(group => group.GroupId == groupId);
        }

        private void SortGraphics(List<GraphicElement> graphics)
        {
            if (graphics == null) return;

            graphics.Sort((x, y) =>
            {
                return x.Graphic.transform.GetSiblingIndex().CompareTo(y.Graphic.transform.GetSiblingIndex());
            });
        }

        public void RefreshGraphicIndices()
        {
            if (_graphics == null || _graphicGroups == null) return;

            SortGraphics(_graphics);

            foreach (var group in _graphicGroups)
            {
                SortGraphics(group.Graphics);
            }
        }

        private void CheckCanvasGroup()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = _viewRoot.gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void OnAnimationComplete(UIAnimationContext context)
        {
            switch (context.AnimationMode)
            {
                case UIAnimationMode.Show:
                    SetVisible(true);
                    break;

                case UIAnimationMode.Hide:
                    SetVisible(false);
                    break;
            }
        }

        public void BackupGraphicState(UIAnimationStateStorage storage)
        {
            CheckCanvasGroup();

            storage.Begin("view");

            //alpha
            storage.Set("alpha", _canvasGroup.alpha);

            //anchors
            storage.Set("anchorMin", AnchorMin);
            storage.Set("anchorMax", AnchorMax);
        }

        public void RestoreGraphicState(UIAnimationStateStorage storage)
        {
            storage.Begin("view");

            //alpha
            _canvasGroup.alpha = storage.Get<float>("alpha");

            //anchors
            AnchorMin = storage.Get<Vector2>("anchorMin");
            AnchorMax = storage.Get<Vector2>("anchorMax");
        }

        public void SetAlpha(float alpha)
        {
            CheckCanvasGroup();
            _canvasGroup.alpha = alpha;
        }

        public void OnScrollStateChanged(bool scrollable)
        {
            if (_scrollRect == null)
            {
                Debug.LogError($"Cannot update view scroll state, update view!! ({Name})");
                return;
            }

            _scrollRect.enabled = scrollable;

            if (!scrollable)
            {
                //expand viewport
                _scrollRect.viewport.sizeDelta = Vector2.zero;

                //expand content?
                _oldContentSize = _content.sizeDelta;
                TransformUtility.OverrideAnchorsStretched(_content);
            }
            else
            {
                //top-stretch
                _content.anchorMin = new Vector2(0f, 1f);
                _content.anchorMax = new Vector2(1f, 1f);
                _content.sizeDelta = _oldContentSize;
            }

            var vScroll = ScrollSettings.ScrollVertical;
            _scrollRect.vertical = vScroll;
            _scrollRect.verticalScrollbar = vScroll ? _scrollVertical : null;
            _scrollVertical.gameObject.SetActive(scrollable && vScroll);

            var hScroll = ScrollSettings.ScrollHorizontal;
            _scrollRect.horizontal = hScroll;
            _scrollRect.horizontalScrollbar = hScroll ? _scrollHorizontal : null;
            _scrollHorizontal.gameObject.SetActive(scrollable && hScroll);
        }
    }
}
