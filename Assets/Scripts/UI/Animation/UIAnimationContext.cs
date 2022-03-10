using System;

namespace MRK.UI.Animation
{
    public class UIAnimationContext
    {
        private readonly object[] _contextArgs;
        private readonly UIAnimationStateStorage _stateStorage;

        public event Action<UIAnimationContext> OnAnimationComplete;

        public IUIAnimatable Target
        {
            get;
            private set;
        }

        public float Length
        {
            get;
            private set;
        }

        public UIAnimationMode AnimationMode
        {
            get;
            private set;
        }

        public bool IsShowing
        {
            get { return AnimationMode == UIAnimationMode.Show; }
        }

        public UIAnimationStateStorage StateStorage
        {
            get { return _stateStorage; }
        }

        public UIAnimationContext(IUIAnimatable target, float length, UIAnimationMode mode, object[] contextArgs)
        {
            Target = target;
            Length = length;
            AnimationMode = mode;

            _contextArgs = contextArgs ?? new object[0];
            _stateStorage = new UIAnimationStateStorage();
        }

        public void OnComplete()
        {
            if (OnAnimationComplete != null)
            {
                OnAnimationComplete(this);
            }
        }

        public T GetArgument<T>(int occurrence)
        {
            if (_contextArgs.Length == 0) return default;

            int occ = 0;
            foreach (var obj in _contextArgs)
            {
                if (obj is T && occ++ == occurrence)
                {
                    return (T)obj;
                }
            }

            return default;
        }
    }
}
