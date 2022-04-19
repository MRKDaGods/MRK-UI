using MRK.UI.Animation;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MRK.UI
{
    public class Layout : MonoBehaviour, IUIAnimatable, IUIAnimatableAlpha, IUIAnimatableMove
    {
        [SerializeField]
        private LayoutRegistry _registry;
        private LayoutType? _layoutType;
        private LayoutIdentifier? _identifier;
        [SerializeField]
        private int _layer;
        [SerializeField]
        private bool _requireSpecializedLayer;
        [SerializeField]
        private List<View> _views;
        private bool _visible;
        private bool _initialized;
        private CanvasGroup _canvasGroup;
        private CachedRectTransform _cachedRectTransform;
        private readonly UIAnimator _animator;
        private LayoutVisiblityContext _visiblityContext;

        public LayoutManager LayoutManager
        {
            get { return LayoutManager.Instance; }
        }

#if UNITY_EDITOR
        public Dictionary<string, object> EditorStorage
        {
            get; private set;
        }
#endif

        public LayoutType LayoutType
        {
            get { return _layoutType ??= GetLayoutType(); }
        }

        public LayoutIdentifier Identifier
        {
            get { return _identifier ??= GetIdentifier(); }
        }

        public int Layer
        {
            get { return _layer; }
            set
            {
                if (_layer != value)
                {
                    LayoutManager.ContainerManager.MarkLayerDirty(_layer);

                    _layer = value;
                    LayoutManager.AdjustLayoutToLayer(this);
                }
            }
        }

        public bool RequireSpecializedLayer
        {
            get { return _requireSpecializedLayer; }
        }

        public List<View> Views
        {
            get { return _views; }
        }

        public bool Visible
        {
            get { return _visible; }
        }

        public LayoutRegistry LayoutRegistry
        {
            get { return _registry; }
        }

        public bool Initialized
        {
            get { return _initialized; }
        }

        public UIAnimator Animator
        {
            get { return _animator; }
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

        public LayoutVisiblityContext VisiblityContext
        {
            get { return _visiblityContext ??= new LayoutVisiblityContext(this); }
        }

        public Layout()
        {
            _animator = new UIAnimator(this);
        }

        public void Initialize()
        {
            if (_initialized) return;

            _initialized = true;

            //store recttransform
            _cachedRectTransform = new CachedRectTransform(gameObject);

            CheckViews();

            //initialize views
            _views.ForEach(view => view.Initialize(this));

            //initialize
            OnLayoutInitialize(new ViewElementInitializer(this));

            //disable
            //gameObject.SetActive(false);
            SetVisible(false);
        }

#if UNITY_EDITOR
        public void InitEditorStorage()
        {
            EditorStorage = new Dictionary<string, object>
            {
                {
                    "foldout",
                    false
                },
                {
                    "so0", //serializedObject 0
                    new SerializedObject(this)
                },
                {
                    "foldoutV",
                    false
                }
            };
        }
#endif

        private void CheckViews()
        {
            if (_views == null)
            {
                _views = new List<View>();
            }
        }

        private LayoutType GetLayoutType()
        {
            var desc = GetType().GetCustomAttribute<LayoutDescriptor>();
            if (desc != null) return desc.LayoutType;

            return LayoutType.None;
        }

        private LayoutIdentifier GetIdentifier()
        {
            var desc = GetType().GetCustomAttribute<LayoutDescriptor>();
            if (desc != null) return desc.Identifier;

            return new LayoutIdentifier(LayoutType, "no-identifier");
        }

        public View GetView(string name)
        {
            CheckViews();
            return _views.Find(view => view.Name == name);
        }

        public void RemoveView(View view)
        {
            if (!_views.Contains(view)) return;

            view.Destroy();
            _views.Remove(view);
        }

        public View GetViewFromGraphic(GraphicElement graphic)
        {
            if (graphic == null) return null;

            return _views.Find(view =>
            {
                if (graphic.SafeHasGroup)
                {
                    var group = view.GetGraphicGroup(graphic.GroupId);
                    return group != null; // && group.Graphics.Contains(graphic);
                }

                return view.Graphics.Find(gfx => gfx == graphic) != null;
            });
        }

        public void Show()
        {
            if (_visible || _animator.IsAnimatingShow) return;

            SetVisible(true);
        }

        public void Show(UIAnimation animation, params object[] contextArgs)
        {
            if (_visible || _animator.IsAnimatingShow) return;

            SetVisible(true, false);
            _animator.PlayAnimation(animation, UIAnimationMode.Show, contextArgs);
        }

        public void Hide()
        {
            if (!_visible || _animator.IsAnimatingHide) return;

            SetVisible(false);
        }

        public void Hide(UIAnimation animation, params object[] contextArgs)
        {
            if (!_visible || _animator.IsAnimatingHide) return;

            _animator.PlayAnimation(animation, UIAnimationMode.Hide, contextArgs);
        }

        private void SetVisible(bool visible, bool updateLocalState = true)
        {
            if (updateLocalState)
            {
                _visible = visible;
            }

            gameObject.SetActive(visible);
            LayoutManager.ContainerManager.UpdateLayerState(Layer);
        }

        private void Update()
        {
            if (!_visible) return;

            OnLayoutUpdate();
        }

        private void CheckCanvasGroup()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void SetRTL(bool rtl)
        {
            _views.ForEach(view => view.SetRTL(rtl));
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

            storage.Begin("layout");

            //alpha
            storage.Set("alpha", _canvasGroup.alpha);

            //anchors
            storage.Set("anchorMin", AnchorMin);
            storage.Set("anchorMax", AnchorMax);
        }

        public void RestoreGraphicState(UIAnimationStateStorage storage)
        {
            storage.Begin("layout");

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

        protected virtual void OnLayoutInitialize(ViewElementInitializer initializer)
        {
        }

        protected virtual void OnLayoutUpdate()
        {
        }
    }
}
