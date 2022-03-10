using MRK.Utils;
using UnityEngine;

namespace MRK.UI
{
    public class UICamera
    {
        private const string UICameraObjectName = "UI Camera";

        private Camera _camera;
        private GameObject _cameraObj;
        private RenderTexture _renderTexture;

        public Camera Camera
        {
            get { return _camera; }
        }

        public RenderTexture RenderTexture
        {
            get { return _renderTexture; }
        }

        public void Setup()
        {
            if (_cameraObj == null)
            {
                _cameraObj = GameObject.Find(UICameraObjectName);
                if (_cameraObj == null)
                {
                    _cameraObj = new GameObject(UICameraObjectName);
                }
            }

            if (_camera == null)
            {
                _camera = _cameraObj.GetComponent<Camera>();

                if (_camera == null)
                {
                    _camera = _cameraObj.AddComponent<Camera>();
                }
            }

            _camera.cullingMask = LayerMask.GetMask("UI");
            _camera.clearFlags = CameraClearFlags.Nothing;
            //_renderTexture = new RenderTexture(w, h, 24);
            //_camera.targetTexture = _renderTexture;

            //update canvases
            //CanvasSettings.UICamera = _camera;
        }

        public void Clean()
        {
            if (_cameraObj != null)
            {
                ObjectUtility.SafeDestroy(_cameraObj);
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                _renderTexture = null;
            }
        }
    }
}
