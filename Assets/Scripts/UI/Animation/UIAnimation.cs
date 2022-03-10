using DG.Tweening;

namespace MRK.UI.Animation
{
    public abstract class UIAnimation
    {
        private float _progress;
        private Tween _tween;
        private UIAnimationContext _context;

        protected float Progress
        {
            get { return _progress; }
        }

        protected IUIAnimatable Target
        {
            get { return _context.Target; }
        }

        public UIAnimationContext Context
        {
            get { return _context; }
        }

        public void Play(UIAnimationContext context)
        {
            _context = context;

            //backup states
            context.StateStorage.SetStorageMode(true);
            Target.BackupGraphicState(context.StateStorage);

            //init anim
            OnAnimationInit();

            _progress = 0f;
            _tween = DOTween.To(
                () => _progress,
                (val) => _progress = val,
                1f,
                _context.Length
            );

            _tween.OnUpdate(OnAnimationUpdate);
            _tween.OnKill(OnKill);
        }

        public void Stop()
        {
            _tween.Kill();
        }

        private void OnKill()
        {
            UIAnimationManager.Instance.Release(this);

            _context.StateStorage.SetStorageMode(false);
            Target.RestoreGraphicState(_context.StateStorage);

            _context = null;
        }

        public static implicit operator UIAnimation(System.Type type)
        {
            return UIAnimationManager.Instance.Get(type);
        }

        protected virtual void OnAnimationInit()
        {
        }

        protected abstract void OnAnimationUpdate();
    }
}
