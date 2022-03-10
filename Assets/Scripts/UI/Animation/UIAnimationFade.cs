namespace MRK.UI.Animation
{
    public class UIAnimationFade : UIAnimation
    {
        private IUIAnimatableAlpha _animatable;

        protected override void OnAnimationInit()
        {
            _animatable = Target as IUIAnimatableAlpha;
            if (_animatable == null)
            {
                throw new System.Exception($"{Target} is not IUIAnimatableAlpha");
            }

            _animatable.SetAlpha(Context.IsShowing ? 0f : 1f);
        }

        protected override void OnAnimationUpdate()
        {
            _animatable.SetAlpha(Context.IsShowing ? Progress : 1f - Progress);
        }
    }
}
