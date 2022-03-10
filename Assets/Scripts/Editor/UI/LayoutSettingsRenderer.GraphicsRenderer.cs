using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MRK.UI
{
    public partial class LayoutSettingsRenderer : SettingsRenderer
    {
        private GUIStyle _graphicStyle;
        private GUIStyle _graphicAddStyle;
        private bool _graphicAddFoldout;
        private readonly HashSet<GraphicElement> _graphicRemovalQueue;

        private void RenderViewGraphics(View view)
        {
            if (view == null) return;

            //add
            UIEditor.RenderVerticalFoldoutNested(
                ref _graphicAddFoldout,
                "Add",
                () =>
                {
                    for (GraphicType graphicType = GraphicType.None + 1; graphicType < GraphicType.MAX; graphicType++)
                    {
                        if (GUILayout.Button($"Add <b>{graphicType}</b>", _graphicAddStyle))
                        {
                            GraphicManager.Instance.AddGraphicToView(view, graphicType);
                        }
                    }
                },
                _graphicStyle
            );

            if (view.Graphics != null)
            {
                foreach (var graphic in view.Graphics)
                {
                    RenderGraphic(graphic);
                }

                if (_graphicRemovalQueue.Count > 0)
                {
                    _graphicRemovalQueue.RemoveWhere(graphic =>
                    {
                        GraphicManager.Instance.RemoveGraphicFromView(view, graphic);
                        return true;
                    });
                }
            }
        }

        private void RenderGraphic(GraphicElement graphic)
        {
            if (graphic == null) return;

            GUILayout.BeginVertical(_graphicStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"<b>{graphic.Graphic.name} (<color=yellow>{graphic.GraphicType}</color>)</b>", _graphicStyle))
            {
                OnGraphicSelected(graphic);
            }

            if (GUILayout.Button("X", GUILayout.Width(50f)))
            {
                _graphicRemovalQueue.Add(graphic);
            }

            GUILayout.EndHorizontal();

            RenderGraphicData(graphic);

            GUILayout.EndVertical();
        }

        private void OnGraphicSelected(GraphicElement graphic)
        {
            Selection.activeGameObject = graphic.Graphic.gameObject;

            SceneSettings.BeginEdit();
            SceneSettings.Focus(graphic.Graphic.transform.position);

            UIEditor.SetActiveGraphic(graphic);
        }

        private void RenderGraphicData(GraphicElement graphic)
        {
            if (graphic == null) return;

            graphic.Graphic.name = EditorGUILayout.TextField("Name", graphic.Graphic.name);

            if (graphic.GraphicType == GraphicType.Group)
            {
                RenderGroupData(graphic.GroupData);
            }
        }

        private void RenderGroupData(GraphicGroup group)
        {
            if (group == null) return;

            UIEditor.RenderVerticalFoldoutNested(
                ref group.EditorAddFoldout,
                "Add",
                () =>
                {
                    for (GraphicType graphicType = GraphicType.None + 1; graphicType < GraphicType.MAX; graphicType++)
                    {
                        if (graphicType == GraphicType.Group) continue;

                        if (GUILayout.Button($"Add <b>{graphicType}</b>", _graphicAddStyle))
                        {
                            GraphicManager.Instance.AddGraphicToGraphicGroup(group, graphicType);
                        }
                    }
                },
                _graphicStyle
            );

            if (group.Graphics != null)
            {
                UIEditor.RenderVerticalFoldoutNested(
                    ref group.EditorGraphicsFoldout,
                    "Graphics",
                    () =>
                    {
                        foreach (GraphicElement graphicElement in group.Graphics)
                        {
                            RenderGraphic(graphicElement);
                        }
                    },
                    _graphicStyle
                );
            }
        }
    }
}
