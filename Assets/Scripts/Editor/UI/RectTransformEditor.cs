using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MRK.UI
{
    [CustomEditor(typeof(RectTransform))]
    public class RectTransformEditor : Editor
    {
        private class GUIState
        {
            public bool RichTextEnabled;
            public bool Enabled;
        }

        private Editor editorInstance;
        private Type nativeEditor;
        private MethodInfo onSceneGui;
        private MethodInfo onValidate;
        private GUIState m_CurrentState;
        private string m_CurrentGroup;
        private int m_GroupDepth;

        private RectTransform RectTransform
        {
            get { return (RectTransform)target; }
        }

        private void OnEnable()
        {
            if (nativeEditor == null)
                Initialize();

            nativeEditor.GetMethod("OnEnable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(editorInstance, null);
            onSceneGui = nativeEditor.GetMethod("OnSceneGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            onValidate = nativeEditor.GetMethod("OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void OnSceneGUI()
        {
            onSceneGui.Invoke(editorInstance, null);
        }

        private void OnDisable()
        {
            nativeEditor.GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(editorInstance, null);
        }

        private void Awake()
        {
            Initialize();
            nativeEditor.GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(editorInstance, null);
        }

        private void Initialize()
        {
            nativeEditor = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.RectTransformEditor");
            editorInstance = CreateEditor(target, nativeEditor);
        }

        private void OnDestroy()
        {
            nativeEditor.GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(editorInstance, null);

            DestroyImmediate(editorInstance);
        }

        private void OnValidate()
        {
            if (nativeEditor == null)
                Initialize();

            onValidate?.Invoke(editorInstance, null);
        }

        private void Reset()
        {
            if (nativeEditor == null)
                Initialize();

            nativeEditor.GetMethod("Reset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(editorInstance, null);
        }

        public override void OnInspectorGUI()
        {
            editorInstance.OnInspectorGUI();

            SaveCurrentGUIState();

            GUI.skin.label.richText = true;

            BeginGroup("Properties", "rectTransform");
            {
                DrawProperty("offsetMin", RectTransform.offsetMin);
                DrawProperty("offsetMax", RectTransform.offsetMax);
                DrawProperty("sizeDelta", RectTransform.sizeDelta);
                DrawProperty("anchoredPosition", RectTransform.anchoredPosition);
            }
            EndGroup();

            BeginGroup("Edit", "rectTransform");
            {
                RectTransform.sizeDelta = EditorGUILayout.Vector2Field("\tsizeDelta", RectTransform.sizeDelta);
            }
            EndGroup();

            RestoreGUIState();
        }

        private void SaveCurrentGUIState()
        {
            if (m_CurrentState == null)
                m_CurrentState = new GUIState();

            m_CurrentState.RichTextEnabled = GUI.skin.label.richText;
            m_CurrentState.Enabled = GUI.enabled;
        }

        private void RestoreGUIState()
        {
            if (m_CurrentState == null)
                return;

            GUI.skin.label.richText = m_CurrentState.RichTextEnabled;
            GUI.enabled = m_CurrentState.Enabled;
        }

        private void BeginGroup(string properties, string group)
        {
            m_CurrentGroup = group;

            if (m_GroupDepth > 0)
                GUILayout.Label("");

            GUILayout.Label($"{new string('\t', m_GroupDepth)}<b>{properties}</b>:");

            m_GroupDepth++;
        }

        private void EndGroup()
        {
            m_GroupDepth--;
        }

        private void DrawProperty<T>(string prop, T value) where T : struct
        {
            GUILayout.Label($"{new string('\t', m_GroupDepth)}{m_CurrentGroup}.<b>{prop}</b>: <b><color=white>{value}</color></b>");
        }
    }
}