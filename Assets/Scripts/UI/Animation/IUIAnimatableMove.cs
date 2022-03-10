using UnityEngine;

namespace MRK.UI.Animation
{
    public interface IUIAnimatableMove : IUIAnimatable
    {
        public Vector2 AnchorMin
        {
            get;
            set;
        }

        public Vector2 AnchorMax
        {
            get;
            set;
        }
    }
}
