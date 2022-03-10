using MRK.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MRK.UI
{
    [Serializable]
    public class GraphicGroup
    {
        [SerializeField]
        private List<GraphicElement> _graphics;
        [SerializeField]
        private GraphicElement _graphic;
        [SerializeField]
        private int _groupId;

#if UNITY_EDITOR
        public bool EditorAddFoldout;
        public bool EditorGraphicsFoldout;
#endif

        public List<GraphicElement> Graphics
        {
            get
            {
                CheckGraphics();
                return _graphics;
            }
        }

        public GraphicElement GraphicElement
        {
            get { return _graphic; }
            set { _graphic = value; }
        }

        public int GroupId
        {
            get { return _groupId; }
        }

        private void CheckGraphics()
        {
            if (_graphics == null)
            {
                _graphics = new List<GraphicElement>();
            }
        }

        public void Initialize(GraphicElement graphicElement)
        {
            GraphicElement = graphicElement;

            do 
                _groupId = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            while (_groupId == -1 || _groupId == 0);
        }

        public void RemoveGraphic(GraphicElement graphic)
        {
            if (!_graphics.Remove(graphic)) return;

            ObjectUtility.SafeDestroy(graphic.Graphic.gameObject);
        }
    }
}
