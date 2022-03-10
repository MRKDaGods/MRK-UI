using System;
using UnityEngine;
using UnityEngine.UI;

namespace MRK.UI
{
    [Serializable]
    public class GraphicElement
    {
        [SerializeField]
        private GraphicType _graphicType;
        [SerializeField]
        private Graphic _graphic;
        [NonSerialized]
        private View _view;
        [SerializeField]
        private int _groupId = -1;
        [NonSerialized]
        private GraphicGroup _groupData;

        public GraphicType GraphicType
        {
            get { return _graphicType; }
            set { _graphicType = value; }
        }

        public Graphic Graphic
        {
            get { return _graphic; }
            set { _graphic = value; }
        }

        public View View
        {
            get
            {
                CheckView();
                return _view;
            }
            set { _view = value; }
        }

        public GraphicGroup GroupData
        {
            get { return _groupData ?? View.GetGraphicGroup(_groupId); }
            set
            {
                _groupId = value?.GroupId ?? -1;
                _groupData = null;
            }
        }

        public bool HasGroup
        {
            get { return GroupData != null && GroupData.GraphicElement != this; }
        }

        public bool SafeHasGroup
        {
            get { return _groupId != -1 && _groupId != 0; }
        }

        public int GroupId
        {
            get { return _groupId; }
        }

        public override bool Equals(object obj)
        {
            return obj is GraphicElement element && element.Graphic == _graphic;
        }

        public override int GetHashCode()
        {
            return _graphic.GetHashCode();
        }

        private void CheckView()
        {
            if (_view == null)
            {
                //Layout
                //--View
                //----graphic
                _view = Graphic.GetComponentInParent<Layout>(true).GetViewFromGraphic(this);

                if (_view == null)
                {
                    Debug.Log($"Cannot find view for {Graphic}");
                }
            }
        }
    }
}
