using System;
using UnityEngine;

namespace MRK.UI
{
    [Serializable]
    public class ScrollSettings
    {
        [SerializeField]
        private bool _scrollable;
        [SerializeField]
        private bool _scrollVertical;
        [SerializeField]
        private bool _scrollHorizontal;

        public Action<bool> OnScrollChanged;

        public bool Scrollable
        {
            get { return _scrollable; }
            set
            {
                if (_scrollable != value)
                {
                    _scrollable = value;

                    if (OnScrollChanged != null)
                    {
                        OnScrollChanged(value);
                    }
                }
            }
        }

        public bool ScrollVertical
        {
            get { return _scrollVertical; }
            set
            {
                if (_scrollVertical != value)
                {
                    _scrollVertical = value;

                    if (OnScrollChanged != null)
                    {
                        OnScrollChanged(_scrollable);
                    }
                }
            }
        }

        public bool ScrollHorizontal
        {
            get { return _scrollHorizontal; }
            set
            {
                if (_scrollHorizontal != value)
                {
                    _scrollHorizontal = value;

                    if (OnScrollChanged != null)
                    {
                        OnScrollChanged(_scrollable);
                    }
                }
            }
        }
    }
}
