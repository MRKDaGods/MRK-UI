using System.Collections.Generic;

namespace MRK.UI.Animation
{
    public class UIAnimationState
    {
        private readonly Dictionary<string, object> _storage;

        public string State
        {
            get;
            private set;
        }

        public UIAnimationState(string state)
        {
            State = state;

            _storage = new Dictionary<string, object>();
        }

        public T Get<T>(string key)
        {
            return _storage.TryGetValue(key, out object value) ? (T)value : default;
        }

        public void Set<T>(string key, T val)
        {
            _storage[key] = val;
        }
    }
}
