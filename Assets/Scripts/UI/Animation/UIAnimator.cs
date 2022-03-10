namespace MRK.UI.Animation
{
    public class UIAnimator
    {
        private readonly IUIAnimatable _animatable;
        private UIAnimation _currentAnimation;
        private UIAnimationMode _currentAnimationMode;

        public bool IsAnimatingShow
        {
            get { return _currentAnimation != null && _currentAnimationMode == UIAnimationMode.Show; }
        }

        public bool IsAnimatingHide
        {
            get { return _currentAnimation != null && _currentAnimationMode == UIAnimationMode.Hide; }
        }

        public UIAnimationMode AnimationMode
        {
            get { return _currentAnimationMode; }
        }

        public float CurrentAnimationLength
        {
            get { return _currentAnimation.Context.Length; }
        }

        public UIAnimator(IUIAnimatable animatable)
        {
            _animatable = animatable;
        }

        public void PlayAnimation(UIAnimation animation, UIAnimationMode mode, params object[] contextArgs)
        {
            if (animation == null) return;

            //what if an anim is currently playing?
            if (_currentAnimation != null)
            {
                _currentAnimation.Stop();
            }

            _currentAnimation = animation;
            _currentAnimationMode = mode;

            var context = UIAnimationManager.Instance.Animate(_animatable, animation, mode, contextArgs: contextArgs);
            context.OnAnimationComplete += _animatable.OnAnimationComplete;
            context.OnAnimationComplete += OnAnimationComplete;
        }

        private void OnAnimationComplete(UIAnimationContext context)
        {
            _currentAnimation = null;
            _currentAnimationMode = UIAnimationMode.None;
        }
    }
}
