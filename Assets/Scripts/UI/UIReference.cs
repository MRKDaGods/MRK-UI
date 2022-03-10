using UnityEngine.UI;

namespace MRK.UI
{
    public class UIReference<T> : Reference<T> where T : Graphic
    {
        public UIReference(T value) : base(value)
        {
        }

        public static implicit operator T(UIReference<T> reference)
        {
            return reference?.Value;
        }
    }
}
