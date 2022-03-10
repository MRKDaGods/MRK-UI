namespace MRK.UI.Animation
{
    public interface IUIAnimatable
    {
        public UIAnimator Animator
        {
            get;
        }

        public void OnAnimationComplete(UIAnimationContext context);
        public void BackupGraphicState(UIAnimationStateStorage storage);
        public void RestoreGraphicState(UIAnimationStateStorage storage);
    }
}
