using MRK.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MRK.UI
{
    public class LayoutManager
    {
        private const string EventSystemName = "UI Event System";

        private readonly Dictionary<string, Type> _screenTypeMap;
        private readonly Dictionary<string, ScreenBase> _screens;
        private readonly Dictionary<Type, Layout> _layouts;

        private static LayoutManager _instance;

        public Dictionary<string, ScreenBase>.ValueCollection Screens
        {
            get
            {
                return _screens.Values;
            }
        }

        public Dictionary<Type, Layout>.ValueCollection Layouts
        {
            get
            {
                return _layouts.Values;
            }
        }

        public ContainerManager ContainerManager
        {
            get { return ContainerManager.Instance; }
        }

        public static LayoutManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LayoutManager();
                }

                return _instance;
            }
        }

        public LayoutManager()
        {
            _screenTypeMap = new Dictionary<string, Type>();
            _screens = new Dictionary<string, ScreenBase>();
            _layouts = new Dictionary<Type, Layout>();
        }

        private void CheckSysTypes()
        {
            _screenTypeMap.Clear();

            foreach (Type type in typeof(ScreenBase).Assembly.GetTypes())
            {
                LayoutDescriptor descriptor = type.GetCustomAttribute<LayoutDescriptor>();
                if (descriptor != null)
                {
                    _screenTypeMap[descriptor.Identifier.FullIdentifier] = type;
                }
            }
        }

        private void CheckEventSystem()
        {
            if (GameObject.Find(EventSystemName) == null)
            {
                new GameObject(EventSystemName).AddComponent<EventSystem>();
            }
        }

        public bool CreateScreen(string identifier)
        {
            if (_screens.ContainsKey(identifier)) return false;

            CheckSysTypes();

            if (!_screenTypeMap.TryGetValue(identifier, out Type sysType))
            {
                Debug.LogError($"Cannot find sysType for {identifier}");
                return false;
            }

            //screen base object
            GameObject obj = new GameObject(identifier);
            obj.layer = ContainerManager.UIOnlyLayer;

            ScreenBase screenBase = (ScreenBase)obj.AddComponent(sysType);
            ContainerManager.AddLayout(screenBase);

            TransformUtility.OverrideAnchorsStretched(
                TransformUtility.SwitchToRectTransform(obj.transform)
            );

            _layouts[sysType] = screenBase;
            _screens[identifier] = screenBase;

#if UNITY_EDITOR
            screenBase.InitEditorStorage();
#endif

            return true;
        }

        public bool CreateView(Layout layout, string name)
        {
            if (layout == null) return false;

            View view = layout.GetView(name);
            if (view != null && view.ViewRoot != null) return false;

            if (view == null)
            {
                view = new View
                {
                    Name = name
                };

                layout.Views.Add(view);
            }

            view.Initialize(layout);
            return true;
        }

        public void Initialize(bool initLayouts = true)
        {
            CheckSysTypes();
            CheckEventSystem();

            _screens.Clear();
            _layouts.Clear();

            foreach (var layout in ContainerManager.GetLayouts())
            {
#if UNITY_EDITOR
                layout.InitEditorStorage();
#endif

                _layouts[layout.GetType()] = layout;

                if (layout.LayoutType == LayoutType.Screen)
                {
                    _screens[layout.Identifier.FullIdentifier] = (ScreenBase)layout;
                }


                if (initLayouts && !layout.Initialized)
                {
                    layout.Initialize();
                }
            }

            ContainerManager.UpdateLayersState();
        }

        public void AdjustLayoutToLayer(Layout layout)
        {
            ContainerManager.AdjustLayoutLayer(layout);
        }

        public T GetLayout<T>() where T : Layout
        {
            return _layouts.TryGetValue(typeof(T), out var layout) ? (T)layout : null;
        }
    }
}
