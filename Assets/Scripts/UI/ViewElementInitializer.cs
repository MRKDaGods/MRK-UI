using UnityEngine.UI;

namespace MRK.UI
{
    public class ViewElementInitializer
    {
        private readonly Layout _layout;
        private View _view;

        public ViewElementInitializer(Layout layout)
        {
            _layout = layout;
        }

        public ViewElementInitializer View(ref View view, string name)
        {
            if (_layout == null) return null;

            view = _layout.GetView(name);
            return view != null ? this : null;
        }

        public ViewElementInitializer View(string name)
        {
            return View(ref _view, name);
        }

        public ViewElementInitializer Init<T>(ref UIReference<T> reference, string key) where T : Graphic
        {
            if (_layout == null || _view == null) return null;

            var obj = _view.Layout.LayoutRegistry.Get<T>(key);
            if (obj == null) return null;

            reference = new UIReference<T>(obj);
            return this;
        }
    }
}
