using MRK.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MRK.UI
{
    public class ViewRenderer
    {
        private Rect _wndRect;
        private Rect _canvasRect;
        private Rect _scaledCvRect;
        private Vector2 _scale;
        private UICamera _uiCamera;
        private readonly EditorGizmo _gizmo;
        private Rect? _selectedObjectRect;
        private Graphic _selectedGraphic;
        private bool _mouseDown;

        public View RenderedView
        {
            get; set;
        }

        public ViewRenderer(UICamera uiCamera)
        {
            _uiCamera = uiCamera;
            _gizmo = new EditorGizmo();
            _gizmo.Reset();
        }

        private void BeginRender(Rect windowRect)
        {
            _wndRect = windowRect;
            _canvasRect = new Rect
            {
                x = _wndRect.center.x - CanvasSettings.Width / 2f,
                y = _wndRect.center.y - CanvasSettings.Height / 2f,
                width = CanvasSettings.Width,
                height = CanvasSettings.Height
            };

            _scale = new Vector2(EditorSettings.ViewRendererZoom, EditorSettings.ViewRendererZoom);
            _scaledCvRect = GUIScaleUtility.Scale(_canvasRect, _wndRect.center, _scale);
        }

        public void Draw(Rect windowRect)
        {
            BeginRender(windowRect);

            GUILayout.BeginArea(windowRect);

            UIEditor.Instance.DrawLabel($"Rendering <b>{RenderedView?.Name}</b>");

            GUILayout.EndArea();

            if (RenderedView == null) return;

            Matrix4x4 oldMatrix = GUI.matrix;

            GUIUtility.ScaleAroundPivot(_scale, windowRect.center);

            //draw clipped!!!

            GUIScaleUtility.CheckInit();
            GUIScaleUtility.BeginNoClip();

            GUI.DrawTexture(_canvasRect, _uiCamera.RenderTexture);

            MGL.DrawBox(_canvasRect, Color.red, 2f);

            GUIScaleUtility.RestoreClips();

            GUI.matrix = oldMatrix;

            HandleMouseEvents();

            if (_selectedGraphic != null)
            {
                _gizmo.Render();
            }

            if (_selectedObjectRect.HasValue)
            {
                MGL.DrawBox(_selectedObjectRect.Value, Color.magenta, 1f);
            }
        }

        private void HandleMouseEvents()
        {
            var evt = Event.current;
            if (evt == null) return;

            var mousePos = evt.mousePosition;
            switch (evt.rawType)
            {
                case EventType.MouseDown:
                    if (!_scaledCvRect.Contains(mousePos)) return; //outside

                    _mouseDown = TestCast(RenderedView, PreviewToRealSpace(mousePos), mousePos);
                    UIEditor.Instance.Repaint(); //force repaint
                    break;

                case EventType.MouseDrag:
                    HandleMouseDrag(evt.delta);
                    break;

                case EventType.MouseUp:
                    _mouseDown = false;
                    break;
            }
        }

        private void HandleMouseDrag(Vector2 delta)
        {
            if (!_mouseDown || _selectedGraphic == null) return;

            delta.y *= -1f;
            switch (_gizmo.GetSelectedPart())
            {
                case EditorGizmo.E_SelectedPart.E_SP_AXE_X:
                    _selectedGraphic.rectTransform.localPosition += new Vector3(delta.x, 0f);
                    break;

                case EditorGizmo.E_SelectedPart.E_SP_AXE_Y:
                    _selectedGraphic.rectTransform.localPosition += new Vector3(0f, delta.y);
                    break;

                case EditorGizmo.E_SelectedPart.E_SP_PIVOT:
                    _selectedGraphic.rectTransform.localPosition += (Vector3)delta;
                    break;
            }

            UpdateObjectOverlay();
        }

        private Vector2 PreviewToRealSpace(Vector2 pos)
        {
            return new Vector2(
                (pos.x - _scaledCvRect.x) / _scaledCvRect.width * CanvasSettings.Width,
                CanvasSettings.Height - (pos.y - _scaledCvRect.y) / _scaledCvRect.height * CanvasSettings.Height
            );
        }

        //z - x = y

        private Vector2 RealToPreviewSpace(Vector2 pos)
        {
            return new Vector2(
                pos.x / CanvasSettings.Width * _scaledCvRect.width + _scaledCvRect.x,
                (CanvasSettings.Height - pos.y) / CanvasSettings.Height * _scaledCvRect.height + _scaledCvRect.y - 7.2f * (_scale.x + 0.5f)
            );
        }

        private bool TestCast(View v, Vector3 screenPos, Vector2 normalPos)
        {
            if (_selectedGraphic != null)
            {
                if (_gizmo.IsTouched(normalPos))
                {
                    return true;
                }
            }

            Camera sceneCamera = _uiCamera.Camera;
            Graphic frontGraphic = null;

            for (int i = 0; i < v.Graphics.Count; i++)
            {
                Graphic graphic = v.Graphics[i].Graphic;

                if (!graphic.raycastTarget || graphic.canvasRenderer.cull || graphic.depth == -1)
                    continue;

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, screenPos, sceneCamera, graphic.raycastPadding))
                    continue;

                if (sceneCamera != null && sceneCamera.WorldToScreenPoint(graphic.rectTransform.position).z > sceneCamera.farClipPlane)
                    continue;

                if (graphic.Raycast(screenPos, sceneCamera))
                {
                    if (frontGraphic == null || frontGraphic.depth < graphic.depth)
                    {
                        frontGraphic = graphic;
                    }
                }
            }

            OnObjectSelected(frontGraphic);
            return frontGraphic != null;
        }

        private void OnObjectSelected(Graphic graphic)
        {
            Selection.activeGameObject = graphic != null ? graphic.gameObject : null;

            if (graphic == null)
            {
                _selectedObjectRect = null;
                _selectedGraphic = null;
                _gizmo.Reset();
                return;
            }

            _selectedGraphic = graphic;
            _gizmo.SetGizmoMode(EditorGizmo.E_OperationMode.E_OM_MOVE);
            UpdateObjectOverlay();
        }

        private void UpdateObjectOverlay()
        {
            _selectedObjectRect = RealToPreviewRect(
                RectTransformToScreenSpace(_selectedGraphic.rectTransform)
            );

            _gizmo.SetPos(_selectedObjectRect.Value.center);
        }

        private Rect RectTransformToScreenSpace(RectTransform rectTransform)
        {
            Vector3[] objectCorners = new Vector3[4];
            rectTransform.GetWorldCorners(objectCorners);

            for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
            {
                objectCorners[i] = _uiCamera.Camera.WorldToScreenPoint(objectCorners[i]);
            }

            float x = objectCorners[0].x;
            float y = objectCorners[0].y;
            float w = objectCorners[2].x - x;
            float h = objectCorners[2].y - y;

            return new Rect(x, y, w, h);
        }

        private Rect RealToPreviewRect(Rect rect)
        {
            Vector2 min = RealToPreviewSpace(rect.min);
            Vector2 max = RealToPreviewSpace(rect.max);

            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }
    }
}
