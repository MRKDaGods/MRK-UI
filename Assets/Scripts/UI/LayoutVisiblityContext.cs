using MRK.Threading;
using MRK.UI.Animation;
using System;

namespace MRK.UI
{
    public class LayoutVisiblityContext
    {
        private readonly Layout _layout;
        private float? _animationMaxLength;
        private bool _layoutHideCalled;

        public LayoutVisiblityContext(Layout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException("layout");
            }

            _layout = layout;
        }

        private void SetAnimationEndTime(float duration)
        {
            if (!_animationMaxLength.HasValue || _animationMaxLength.Value < duration)
            {
                _animationMaxLength = duration;
            }
        }

        public LayoutVisiblityContext Begin()
        {
            _animationMaxLength = null;
            _layoutHideCalled = false;

            return this;
        }

        public LayoutVisiblityContext HideAllViews()
        {
            _layout.Views.ForEach(view => view.Hide());
            return this;
        }

        public LayoutVisiblityContext ShowAllViews()
        {
            _layout.Views.ForEach(view => view.Show());
            return this;
        }

        public LayoutVisiblityContext HideView(string name)
        {
            var view = _layout.GetView(name);
            if (view == null) return null;

            view.Hide();
            return this;
        }

        public LayoutVisiblityContext HideViewAnimation(string name, UIAnimation animation, params object[] contextArgs)
        {
            var view = _layout.GetView(name);
            if (view == null) return null;

            view.Hide(animation, contextArgs);
            SetAnimationEndTime(view.Animator.CurrentAnimationLength);

            return this;
        }

        public LayoutVisiblityContext ShowView(string name)
        {
            var view = _layout.GetView(name);
            if (view == null) return null;

            view.Show();
            return this;
        }

        public LayoutVisiblityContext ShowViewAnimation(string name, UIAnimation animation, params object[] contextArgs)
        {
            var view = _layout.GetView(name);
            if (view == null) return null;

            view.Show(animation, contextArgs);
            SetAnimationEndTime(view.Animator.CurrentAnimationLength);

            return this;
        }

        public LayoutVisiblityContext Show()
        {
            _layout.Show();
            return this;
        }

        public LayoutVisiblityContext ShowAnimation(UIAnimation animation, params object[] contextArgs)
        {
            _layout.Show(animation, contextArgs);
            SetAnimationEndTime(_layout.Animator.CurrentAnimationLength);

            return this;
        }

        public LayoutVisiblityContext Hide()
        {
            _layoutHideCalled = true;
            return this;
        }

        public LayoutVisiblityContext HideAnimation(UIAnimation animation, params object[] contextArgs)
        {
            _layout.Hide(animation, contextArgs);
            SetAnimationEndTime(_layout.Animator.CurrentAnimationLength);

            return this;
        }

        public void End()
        {
            //Hide layout after the latest time possible so view-animations would finish
            if (_layoutHideCalled)
            {
                if (_animationMaxLength.HasValue)
                {
                    Runnable.SetupGlobalRunnable();
                    Runnable.Global.RunLater(_layout.Hide, _animationMaxLength.Value);
                }
            }
        }
    }
}
