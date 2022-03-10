using UnityEngine;
using UnityEngine.UI;

namespace MRK.UI.Screens
{
    [LayoutDescriptor(LayoutType.Screen, "ProgressBarTest")]
    public class ProgressBarTest : ScreenBase
    {
        private UIReference<RawImage> _frontImage;
        private Vector2 _initialSize;
        private float _progress;
        private float _w;
        private View _backgroundView;

        protected override void OnLayoutInitialize(ViewElementInitializer initializer)
        {
            initializer.View("ProgressBar")
                .Init(ref _frontImage, "front");

            initializer.View(ref _backgroundView, "Background");

            _initialSize = _frontImage.Value.rectTransform.offsetMax;
            _w = _frontImage.Value.rectTransform.rect.width;
        }

        protected override void OnLayoutUpdate()
        {
            _progress += Time.deltaTime;
            if (_progress > 1f)
            {
                _progress = 0f;
            }

            _frontImage.Value.rectTransform.offsetMax = Vector2.Lerp(new Vector2(_initialSize.x - _w, _initialSize.y), _initialSize, _progress);

            if (Input.GetKeyDown(KeyCode.K))
            {
                _backgroundView.SetVisible(!_backgroundView.Visible);
            }
        }
    }
}
