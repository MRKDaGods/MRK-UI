using System.Collections.Generic;
using UnityEngine;

namespace MRK.UI
{
    public class ContainerManager
    {
        private const string MainContainerName = "UI Container";
        private const string LayoutContainerName = "Layout Container";
        public const int LayerMax = 10;

        private GameObject _mainContainer;
        private GameObject _layoutContainer;
        private readonly List<LayoutLayer> _layoutLayers;
        
        private static ContainerManager _instance;
        private static int? _uiOnlyLayer;

        public static ContainerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ContainerManager();
                }

                return _instance;
            }
        }

        public static int UIOnlyLayer
        {
            get { return _uiOnlyLayer.Value; }
        }

        public ContainerManager()
        {
            _layoutLayers = new List<LayoutLayer>();
        }

        public void CheckConsistency()
        {
            if (_uiOnlyLayer == null)
            {
                _uiOnlyLayer = LayerMask.NameToLayer("UI");
            }

            _mainContainer = GameObject.Find(MainContainerName);
            if (_mainContainer == null)
            {
                _mainContainer = new GameObject(MainContainerName);
            }

            _mainContainer.layer = _uiOnlyLayer.Value;

            _layoutContainer = GameObject.Find($"{MainContainerName}/{LayoutContainerName}");
            if (_layoutContainer == null)
            {
                _layoutContainer = new GameObject(LayoutContainerName);
                _layoutContainer.transform.parent = _mainContainer.transform;
            }

            _layoutContainer.layer = _uiOnlyLayer.Value;

            if (_layoutLayers.Count == 0 || !_layoutLayers.TrueForAll(layout => layout != null && layout.IsValid()))
            {
                if (_layoutLayers.Count > 0)
                {
                    _layoutLayers.ForEach(layout => { if (layout != null) layout.Destroy(); });
                    _layoutLayers.Clear();
                }

                //UI Container
                //--Layout Container
                //----Layer-0
                //------Main
                //------Specialized

                for (int i = 0; i < LayerMax; i++)
                {
                    string layoutName = $"Layer-{i}";
                    var go = GameObject.Find($"{MainContainerName}/{LayoutContainerName}/{layoutName}");
                    if (go == null)
                    {
                        go = new GameObject($"Layer-{i}");
                        go.transform.parent = _layoutContainer.transform;
                    }

                    LayoutLayer layer = new LayoutLayer(go.transform, i);
                    layer.Adjust();

                    _layoutLayers.Add(layer);
                }
            }
        }

        public void AddLayout(Layout layout)
        {
            if (layout == null) return;

            CheckConsistency();
            AdjustLayoutLayer(layout, true);
        }

        public void AdjustLayoutLayer(Layout layout, bool noChecks = false)
        {
            if (!noChecks)
            {
                if (layout == null) return;

                //check!!!
                CheckConsistency();
            }

            _layoutLayers[layout.Layer].MoveLayoutToLayer(layout, layout.RequireSpecializedLayer);
        }

        public Layout[] GetLayouts()
        {
            CheckConsistency();
            return _layoutContainer.GetComponentsInChildren<Layout>(true);
        }

        public ScreenBase[] GetScreens()
        {
            CheckConsistency();
            return _layoutContainer.GetComponentsInChildren<ScreenBase>(true);
        }

        public void EnableLayerSingular(int idx, out LayoutLayer layoutLayer)
        {
            layoutLayer = null;

            if (idx >= _layoutLayers.Count)
            {
                Debug.LogError("Invalid layer index idx=" + idx);
                return;
            }

            for (int i = 0; i < _layoutLayers.Count; i++)
            {
                var layer = _layoutLayers[i];
                if (i == idx)
                {
                    layoutLayer = layer;
                    layer.Root.gameObject.SetActive(true);
                    continue;
                }

                layer.Root.gameObject.SetActive(false);
            }
        }

        public void EnableAllLayers(bool includeChildren = false)
        {
            foreach (var layer in _layoutLayers)
            {
                layer.Root.gameObject.SetActive(true);

                if (includeChildren)
                {
                    layer.EnableAllLayouts();
                }
            }
        }
    }
}
