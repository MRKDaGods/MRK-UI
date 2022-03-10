using UnityEngine;

namespace MRK.UI.Animation
{
    public class UIAnimationMove : UIAnimation
    {
        public enum MoveDirection
        {
            Left,
            Right,
            Up,
            Down
        }

        private IUIAnimatableMove _animatable;

        private static Vector2 _initialAnchorMin;
        private static Vector2 _initialAnchorMax;
        private readonly static Vector2 _targetAnchorMin;
        private readonly static Vector2 _targetAnchorMax;

        private Vector2 InitialAnchorMin
        {
            get { return Context.IsShowing ? _initialAnchorMin : _targetAnchorMin; }
        }

        private Vector2 InitialAnchorMax
        {
            get { return Context.IsShowing ? _initialAnchorMax : _targetAnchorMax; }
        }

        public Vector2 TargetAnchorMin
        {
            get { return Context.IsShowing ? _targetAnchorMin : _initialAnchorMin; }
        }

        public Vector2 TargetAnchorMax
        {
            get { return Context.IsShowing ? _targetAnchorMax : _initialAnchorMax; }
        }

        static UIAnimationMove()
        {
            _initialAnchorMin = new Vector2(-1f, 0f);
            _initialAnchorMax = new Vector2(0f, 1f);
            _targetAnchorMin = new Vector2(0f, 0f);
            _targetAnchorMax = new Vector2(1f, 1f);
        }

        protected override void OnAnimationInit()
        {
            _animatable = Target as IUIAnimatableMove;
            if (_animatable == null)
            {
                throw new System.Exception($"{Target} is not IUIAnimatableMove");
            }

            GetAnchorMoveDirection();

            _animatable.AnchorMin = InitialAnchorMin;
            _animatable.AnchorMax = InitialAnchorMax;
        }

        protected override void OnAnimationUpdate()
        {
            _animatable.AnchorMin = Vector2.Lerp(InitialAnchorMin, TargetAnchorMin, Progress);
            _animatable.AnchorMax = Vector2.Lerp(InitialAnchorMax, TargetAnchorMax, Progress);
        }

        private void GetAnchorMoveDirection()
        {
            var moveDir = Context.GetArgument<MoveDirection>(0);
            switch (moveDir)
            {
                case MoveDirection.Left:
                    _initialAnchorMin = new Vector2(-1f, 0f);
                    _initialAnchorMax = new Vector2(0f, 1f);
                    break;

                case MoveDirection.Right:
                    _initialAnchorMin = new Vector2(1f, 0f);
                    _initialAnchorMax = new Vector2(2f, 1f);
                    break;

                case MoveDirection.Up:
                    _initialAnchorMin = new Vector2(0f, 1f);
                    _initialAnchorMax = new Vector2(1f, 2f);
                    break;

                case MoveDirection.Down:
                    _initialAnchorMin = new Vector2(0f, -1f);
                    _initialAnchorMax = new Vector2(1f, 0f);
                    break;
            }
        }
    }
}
