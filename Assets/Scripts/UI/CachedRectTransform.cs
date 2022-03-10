using UnityEngine;

namespace MRK.UI
{
    public class CachedRectTransform
    {
        private readonly GameObject _obj;
        private RectTransform _rectTransform;

        public RectTransform RectTransform
        {
            get { return GetRectTransform(); }
        }

        public CachedRectTransform(GameObject obj)
        {
            _obj = obj;
        }

        private RectTransform GetRectTransform()
        {
            if (_obj == null) return null;

            if (_rectTransform != null) return _rectTransform;

            _rectTransform = _obj.GetComponent<RectTransform>();
            return _rectTransform;
        }

        public static implicit operator RectTransform(CachedRectTransform cached)
        {
            return cached.RectTransform;
        }
    }
}
