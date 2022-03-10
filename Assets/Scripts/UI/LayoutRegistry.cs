using System;
using System.Collections.Generic;
using UnityEngine;

namespace MRK.UI
{
    [Serializable]
    public class LayoutRegistry
    {
        [Serializable]
        public struct LayoutOption<T>
        {
            public string Key;
            public T Value;
        }

        [SerializeField]
        private List<LayoutOption<bool>> _booleans;
        [SerializeField]
        private List<LayoutOption<string>> _strings;
        [SerializeField]
        private List<LayoutOption<GameObject>> _gameObjects;

        public T Get<T>(string key)
        {
            var t = typeof(T);

            if (t == typeof(bool))
            {
                return (T)(object)_booleans.Find(v => v.Key == key).Value;
            }

            if (t == typeof(string))
            {
                return (T)(object)_strings.Find(v => v.Key == key).Value;
            }

            if (t == typeof(GameObject))
            {
                return (T)(object)_gameObjects.Find(v => v.Key == key).Value;
            }

            if (typeof(MonoBehaviour).IsAssignableFrom(t))
            {
                var go = Get<GameObject>(key);
                if (go != null)
                {
                    return go.GetComponent<T>();
                }
            }

            return default;
        }
    }
}
