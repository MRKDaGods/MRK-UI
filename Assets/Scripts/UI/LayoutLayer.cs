using MRK.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MRK.UI
{
    public class LayoutLayer
    {
        private const string MainLayerName = "Main";
        private const string SpecializedLayerName = "Specialized";

        private Transform _mainLayer;
        private Transform _specializedLayer; //specialized layers are for layouts that require constant updating
        private Layout[] _layouts;

        public Transform Root
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }

        public LayoutLayer(Transform root, int index)
        {
            Root = root;
            Index = index;
        }

        public bool IsValid()
        {
            return Root != null && _mainLayer != null && _specializedLayer != null;
        }

        public void Destroy()
        {
            if (_mainLayer != null)
            {
                Object.Destroy(_mainLayer.gameObject);
            }

            if (_specializedLayer != null)
            {
                Object.Destroy(_specializedLayer.gameObject);
            }

            if (Root != null)
            {
                Object.Destroy(Root.gameObject);
            }
        }

        private void AdjustUIObject(GameObject obj)
        {
            obj.layer = ContainerManager.UIOnlyLayer;
            TransformUtility.ResetLocals(obj.transform);
        }

        public void Adjust()
        {
            Root.SetSiblingIndex(Index);
            AdjustUIObject(Root.gameObject);

            if (_mainLayer == null)
            {
                _mainLayer = Root.Find(MainLayerName);

                if (_mainLayer == null)
                {
                    _mainLayer = new GameObject(MainLayerName).transform;
                    _mainLayer.parent = Root;
                }
            }

            AdjustUIObject(_mainLayer.gameObject);

            if (_specializedLayer == null)
            {
                _specializedLayer = Root.Find(SpecializedLayerName);

                if (_specializedLayer == null)
                {
                    _specializedLayer = new GameObject(SpecializedLayerName).transform;
                    _specializedLayer.parent = Root;
                }
            }

            AdjustUIObject(_specializedLayer.gameObject);

            CheckCanvas(ref _mainLayer);
        }

        private void CheckCanvas(ref Transform layer)
        {
            if (layer.gameObject.GetComponent<Canvas>() == null)
            {
                var cv = layer.gameObject.AddComponent<Canvas>();
                cv.renderMode = RenderMode.ScreenSpaceOverlay;
                cv.sortingOrder = Index;
                cv.pixelPerfect = false;

                //layer gets nulled out for some reason??
                //UPDATE: unity replaces transform with RectTransform upon adding canvas
                layer = cv.transform;
            }

            if (layer.gameObject.GetComponent<CanvasScaler>() == null)
            {
                layer.gameObject.AddComponent<CanvasScaler>();
            }

            if (layer.gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                layer.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        public void MoveLayoutToLayer(Layout layout, bool isSpecialized)
        {
            layout.transform.parent = isSpecialized ? _specializedLayer : _mainLayer;
            TransformUtility.ResetLocals(layout.transform);
        }

        private void CheckLayouts()
        {
            if (_layouts == null)
            {
                _layouts = Root.GetComponentsInChildren<Layout>(true);
            }
        }

        public void EnableLayoutSingular(Layout layout)
        {
            if (layout == null) return;

            CheckLayouts();

            foreach (var l in _layouts)
            {
                l.gameObject.SetActive(l.Identifier == layout.Identifier);
            }
        }

        public void EnableAllLayouts()
        {
            CheckLayouts();

            foreach (var layout in _layouts)
            {
                layout.gameObject.SetActive(true);
            }
        }
    }
}
