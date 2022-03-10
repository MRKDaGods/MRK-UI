using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace MRK.UI
{
    public class CanvasSettings
    {
        private class CanvasInfo
        {
            public Canvas Canvas;
            public CanvasScaler Scaler;
        }

        private static CanvasInfo[] _detectedCanvases;
        private static Camera _uiCamera;

        public static float Width
        {
            get; set;
        }

        public static float Height
        {
            get; set;
        }

        public static float ScreenMatchMode
        {
            get; set;
        }

        public static Camera UICamera
        {
            get
            {
                if (_uiCamera == null)
                {
                    _uiCamera = Camera.main;
                }

                return _uiCamera;
            }

            set
            {
                if (_uiCamera != value)
                {
                    _uiCamera = value;
                    ApplyChanges();
                }
            }
        }

        public static bool CanvasDetected()
        {
            return _detectedCanvases != null && _detectedCanvases.Length > 0;
        }

        public static void Refresh()
        {
            _detectedCanvases = Object.FindObjectsOfType<Canvas>(true).Select(c =>
            {
                var cs = c.GetComponent<CanvasScaler>();
                if (cs != null)
                {
                    return new CanvasInfo { Scaler = cs, Canvas = c };
                }

                return null;
            }).Where(cnvInfo => cnvInfo != null).ToArray();

            if (CanvasDetected())
            {
                var refRes = _detectedCanvases[0].Scaler.referenceResolution;
                Width = refRes.x;
                Height = refRes.y;
                ScreenMatchMode = _detectedCanvases[0].Scaler.matchWidthOrHeight;
            }
        }

        public static void ApplyChanges()
        {
            if (!CanvasDetected()) return;

            foreach (var info in _detectedCanvases)
            {
                info.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
                info.Canvas.worldCamera = UICamera;
                info.Scaler.referenceResolution = new Vector2(Width, Height);
                info.Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                info.Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                info.Scaler.matchWidthOrHeight = ScreenMatchMode;
            }

            Debug.Log("Applied changes to canvases");
        }
    }
}
