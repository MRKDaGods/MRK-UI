using MRK.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MRK.UI
{
    public class GraphicManager
    {
        private static GraphicManager _instance;

        public static GraphicManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GraphicManager();
                }

                return _instance;
            }
        }

        private GameObject GetTemplate(GraphicType type)
        {
            var res = Resources.Load<GameObject>($"UI/Templates/Internal/{type}");
            return res != null ? Object.Instantiate(res) : null;
        }

        public bool GetObjectWithGraphic(GraphicType graphicType, out GameObject obj, out Graphic graphic)
        {
            var template = GetTemplate(graphicType);

            obj = template != null ? template : new GameObject(graphicType.ToString());
            obj.layer = ContainerManager.UIOnlyLayer;

            graphic = null;

            switch (graphicType)
            {
                case GraphicType.Image:
                    graphic = obj.AddComponent<Image>();
                    break;

                case GraphicType.Text:
                    graphic = obj.AddComponent<TextMeshProUGUI>();
                    break;

                case GraphicType.RawImage:
                    graphic = obj.AddComponent<RawImage>();
                    break;

                case GraphicType.Group:
                    graphic = obj.AddComponent<NonDrawingGraphic>();
                    break;

                case GraphicType.InputField:
                    graphic = obj.GetComponent<Image>();
                    break;

                case GraphicType.ProceduralImage:
                    graphic = obj.AddComponent<ProceduralImage>();
                    break;

                case GraphicType.MaskedImage:
                    graphic = obj.GetComponent<ProceduralImage>();
                    break;

                default:
                    ObjectUtility.SafeDestroy(obj);
                    break;
            }

            return graphic != null;
        }

        public void AddGraphicToView(View view, GraphicType graphicType)
        {
            if (view == null || graphicType == GraphicType.None) return;

            if (!GetObjectWithGraphic(graphicType, out GameObject obj, out Graphic graphic))
            {
                Debug.LogError($"Failed to create ui element of type {graphicType}");
                return;
            }

            if (obj.GetComponent<CanvasRenderer>() == null)
            {
                obj.AddComponent<CanvasRenderer>();
            }

            obj.transform.parent = view.ContentRoot;
            TransformUtility.ResetLocals(obj.transform);

            var graphicElement = new GraphicElement
            {
                GraphicType = graphicType,
                Graphic = graphic,
                View = view
            };

            if (graphicType == GraphicType.Group)
            {
                graphicElement.GroupData = view.GetNewGraphicGroup(graphicElement);
            }

            view.Graphics.Add(graphicElement);
        }

        public void AddGraphicToGraphicGroup(GraphicGroup graphicGroup, GraphicType graphicType)
        {
            if (graphicGroup == null || graphicType == GraphicType.None) return;

            if (!GetObjectWithGraphic(graphicType, out GameObject obj, out Graphic graphic))
            {
                Debug.LogError($"Failed to create ui element of type {graphicType}");
                return;
            }

            obj.transform.parent = graphicGroup.GraphicElement.Graphic.transform;
            TransformUtility.ResetLocals(obj.transform);

            graphicGroup.Graphics.Add(new GraphicElement
            {
                GraphicType = graphicType,
                Graphic = graphic,
                View = graphicGroup.GraphicElement.View,
                GroupData = graphicGroup
            });
        }

        public void RemoveGraphicFromView(View view, GraphicElement graphic)
        {
            if (view == null || graphic == null) return;

            if (graphic.HasGroup)
            {
                graphic.GroupData.RemoveGraphic(graphic);
                return;
            }

            view.RemoveGraphic(graphic);
        }
    }
}
