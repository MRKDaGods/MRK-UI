using MRK.Pools;
using System;
using System.Collections.Generic;

namespace MRK.UI.Animation
{
    public class UIAnimationManager
    {
        private const float DefaultAnimationLength = 0.3f;

        private readonly Dictionary<Type, ObjectPool<UIAnimation>> _animations;

        private static UIAnimationManager _instance;

        public static UIAnimationManager Instance
        {
            get
            {
                return _instance ??= new UIAnimationManager();
            }
        }

        public UIAnimationManager()
        {
            _animations = new Dictionary<Type, ObjectPool<UIAnimation>>();
        }

        private void CheckType(Type type, out ObjectPool<UIAnimation> pool)
        {
            if (!_animations.TryGetValue(type, out pool))
            {
                pool = new ObjectPool<UIAnimation>(null);
                _animations[type] = pool;
            }
        }

        public UIAnimation Get(Type type)
        {
            CheckType(type, out var pool);
            return (UIAnimation)pool.GetDynamic(type);
        }

        public T Get<T>() where T : UIAnimation
        {
            return (T)Get(typeof(T));
        }

        public void Release(UIAnimation animation)
        {
            if (animation == null) return;

            animation.Context.OnComplete();

            CheckType(animation.GetType(), out var pool);
            pool.Release(animation);
        }

        public UIAnimationContext Animate(
            IUIAnimatable animatable,
            UIAnimation animation,
            UIAnimationMode animationMode,
            float animLength = DefaultAnimationLength,
            object[] contextArgs = null)
        {
            if (animatable == null || animation == null || animationMode == UIAnimationMode.None) return null;

            var context = new UIAnimationContext(
                animatable,
                animLength,
                animationMode,
                contextArgs
            );
            animation.Play(context);
            return context;
        }
    }
}
