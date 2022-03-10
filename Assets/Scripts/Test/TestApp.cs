using MRK.UI;
using MRK.UI.Animation;
using UnityEngine;

namespace MRK
{
    public class TestApp : MonoBehaviour
    {
        private LoginScreenTest _loginScreenTest;

        private void Start()
        {
            LayoutManager.Instance.Initialize();

            _loginScreenTest = LayoutManager.Instance.GetLayout<LoginScreenTest>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (!_loginScreenTest.Visible)
                {
                    _loginScreenTest.VisiblityContext
                        .Begin()
                        .HideAllViews()
                        .ShowView("Background")
                        .ShowViewAnimation("Main", typeof(UIAnimationMove), UIAnimationMove.MoveDirection.Up)
                        .Show()
                        .End();
                }
                else
                {
                    _loginScreenTest.VisiblityContext
                        .Begin()
                        .HideViewAnimation("Main", typeof(UIAnimationMove), UIAnimationMove.MoveDirection.Down)
                        .HideViewAnimation("Background", typeof(UIAnimationFade))
                        .HideAnimation(typeof(UIAnimationFade))
                        .End();
                }
            }
        }
    }
}
