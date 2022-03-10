using MRK.Rendering;
using UnityEditor;
using UnityEngine;

namespace MRK
{
    public class EditorGizmo
    {
        public enum E_OperationMode
        {
            E_OM_MOVE,
            E_OM_SCALE,
            E_OM_ROTATE,
            E_OM_SIZE
        };

        public enum E_SelectedPart
        {
            E_SP_NONE,
            E_SP_PIVOT,
            E_SP_AXE_X,
            E_SP_AXE_Y
        };

        private E_OperationMode m_Mode = E_OperationMode.E_OM_MOVE;
        private E_SelectedPart m_SelectedPart = E_SelectedPart.E_SP_NONE;
        private Vector2 m_Pos = new Vector2(0.0f, 0.0f);    // in screen coords
        private float m_Rot = 0.0f; // in radians (Z axes)
        private Vector2 m_Scale = new Vector2(1.0f, 1.0f);
        private float DEFAULT_AXE_LENGTH = 60.0f;
        private float DEFAULT_AXE_TOUCHSIZE = 7.0f;
        private float DEFAULT_ARROWHEAD_LENGTH = 15.0f;
        private float DEFAULT_ARROWHEAD_ANGLE = 165.0f * Mathf.PI / 180.0f;
        private float DEFAULT_SCALEHANDLER_RADIUS = 5.0f;
        private float DEFAULT_POINT_RADIUS = 8.0f;
        private float HALF_PI = Mathf.PI * 0.5f;
        private Color DEFAULT_AXE_X_COLOR = Color.red;
        private Color DEFAULT_AXE_Y_COLOR = Color.green;
        private Color DEFAULT_PIVOT_COLOR = Color.blue;
        private Color DEFAULT_ROTATION_COLOR = Color.grey;
        private Color DEFAULT_SELECTION_COLOR = Color.yellow;

        //
        // ctor
        // 
        public EditorGizmo()
        {
            m_Mode = E_OperationMode.E_OM_MOVE;
        }

        //
        // Reset
        //
        public void Reset()
        {
            m_SelectedPart = E_SelectedPart.E_SP_NONE;
            m_Scale.x = 1.0f;
            m_Scale.y = 1.0f;
            m_Rot = 0.0f;
        }

        //
        // Change mode of gizmo (move, scale, rotate)
        //
        public void SetGizmoMode(E_OperationMode mode)
        {
            m_Mode = mode;
        }

        public EditorGizmo.E_OperationMode GetGizmoMode()
        {
            return m_Mode;
        }

        public EditorGizmo.E_SelectedPart GetSelectedPart()
        {
            return m_SelectedPart;
        }

        public float GetDefaultAxeLength()
        {
            return DEFAULT_AXE_LENGTH;
        }

        //
        // Render called by GUI Editor plugin
        //
        public void Render()
        {
            switch (m_Mode)
            {
                case E_OperationMode.E_OM_MOVE:
                    RenderMoveMode();
                    break;

                case E_OperationMode.E_OM_SCALE:
                    RenderScaleMode();
                    break;

                case E_OperationMode.E_OM_ROTATE:
                    RenderRotateMode();
                    break;

                case E_OperationMode.E_OM_SIZE:
                    RenderSizeMode();
                    break;

                default:
                    Debug.LogError("Unsupported case");
                    break;
            }
        }

        //
        // Set position of gizmo's pivot
        //
        public void SetPos(Vector2 pivot)
        {
            m_Pos = pivot;
        }

        //
        // Set rotation of gizmo in degrees
        //
        public void SetRot(float angleDeg)
        {
            m_Rot = (angleDeg * Mathf.PI) / 180.0f;
        }

        //
        // Set scale of Gizmo
        //
        public void SetScale(Vector2 scale)
        {
            m_Scale = scale;
        }

        //
        // Test if mouse is over some of gizmo's active part
        //
        public bool IsTouched(Vector2 mouseScreenPos)
        {
            switch (m_Mode)
            {
                case E_OperationMode.E_OM_MOVE:
                    return IsTouchedMove(mouseScreenPos);

                case E_OperationMode.E_OM_SCALE:
                    return IsTouchedScale(mouseScreenPos);

                case E_OperationMode.E_OM_ROTATE:
                    return IsTouchedRotate(mouseScreenPos);

                case E_OperationMode.E_OM_SIZE:
                    return IsTouchedSize(mouseScreenPos);

                default:
                    Debug.LogError("Unsupported case");
                    break;
            }

            return false;
        }

        //
        // Render gizmo in move mode
        //
        private void RenderMoveMode()
        {
            DrawArrow((m_SelectedPart == E_SelectedPart.E_SP_AXE_X) ? DEFAULT_SELECTION_COLOR : DEFAULT_AXE_X_COLOR, m_Pos, m_Rot, DEFAULT_AXE_LENGTH * m_Scale.x);
            DrawArrow((m_SelectedPart == E_SelectedPart.E_SP_AXE_Y) ? DEFAULT_SELECTION_COLOR : DEFAULT_AXE_Y_COLOR, m_Pos, m_Rot - HALF_PI, DEFAULT_AXE_LENGTH * m_Scale.y);

            DrawPoint((m_SelectedPart == E_SelectedPart.E_SP_PIVOT) ? DEFAULT_SELECTION_COLOR : DEFAULT_PIVOT_COLOR, m_Pos, DEFAULT_POINT_RADIUS, m_Rot);
        }

        //
        // Render gizmo in scale mode
        //
        private void RenderScaleMode()
        {
            DrawScaleHandler((m_SelectedPart == E_SelectedPart.E_SP_AXE_X) ? DEFAULT_SELECTION_COLOR : DEFAULT_AXE_X_COLOR, m_Pos, m_Rot, DEFAULT_AXE_LENGTH * m_Scale.x);
            DrawScaleHandler((m_SelectedPart == E_SelectedPart.E_SP_AXE_Y) ? DEFAULT_SELECTION_COLOR : DEFAULT_AXE_Y_COLOR, m_Pos, m_Rot - HALF_PI, DEFAULT_AXE_LENGTH * m_Scale.y);

            DrawPoint((m_SelectedPart == E_SelectedPart.E_SP_PIVOT) ? DEFAULT_SELECTION_COLOR : DEFAULT_PIVOT_COLOR, m_Pos, DEFAULT_POINT_RADIUS, m_Rot);
        }

        //
        // Render gizmo in rotate mode
        //
        private void RenderRotateMode()
        {
            for (int i = 0; i < 4; ++i)
            {
                DrawRotHandler((m_SelectedPart == E_SelectedPart.E_SP_PIVOT) ? DEFAULT_SELECTION_COLOR : DEFAULT_ROTATION_COLOR, m_Pos, m_Rot - HALF_PI * i, DEFAULT_AXE_LENGTH);
            }

            DrawPoint((m_SelectedPart == E_SelectedPart.E_SP_PIVOT) ? DEFAULT_SELECTION_COLOR : DEFAULT_PIVOT_COLOR, m_Pos, DEFAULT_POINT_RADIUS, m_Rot);
        }

        //
        // Render gizmo in size mode
        //
        private void RenderSizeMode()
        {
            DrawScaleHandler((m_SelectedPart == E_SelectedPart.E_SP_AXE_X) ? DEFAULT_SELECTION_COLOR : DEFAULT_AXE_X_COLOR, m_Pos, m_Rot, DEFAULT_AXE_LENGTH * m_Scale.x);
            DrawScaleHandler((m_SelectedPart == E_SelectedPart.E_SP_AXE_Y) ? DEFAULT_SELECTION_COLOR : DEFAULT_AXE_Y_COLOR, m_Pos, m_Rot - HALF_PI, DEFAULT_AXE_LENGTH * m_Scale.y);

            DrawPoint((m_SelectedPart == E_SelectedPart.E_SP_PIVOT) ? DEFAULT_SELECTION_COLOR : DEFAULT_PIVOT_COLOR, m_Pos, DEFAULT_POINT_RADIUS, m_Rot);
        }

        //
        // Draw Arrow
        //
        private void DrawArrow(Color c, Vector2 pos0, float angle, float length)
        {
            Handles.color = c;

            Vector2 pos1 = new Vector2(pos0.x + Mathf.Cos(angle) * length, pos0.y + Mathf.Sin(angle) * length);
            MGL.DrawLine(pos0, pos1);

            Vector2 tmpPos = new Vector2(pos1.x + Mathf.Cos(angle + DEFAULT_ARROWHEAD_ANGLE) * DEFAULT_ARROWHEAD_LENGTH, pos1.y + Mathf.Sin(angle + DEFAULT_ARROWHEAD_ANGLE) * DEFAULT_ARROWHEAD_LENGTH);
            MGL.DrawLine(pos1, tmpPos);

            tmpPos = new Vector2(pos1.x + Mathf.Cos(angle - DEFAULT_ARROWHEAD_ANGLE) * DEFAULT_ARROWHEAD_LENGTH, pos1.y + Mathf.Sin(angle - DEFAULT_ARROWHEAD_ANGLE) * DEFAULT_ARROWHEAD_LENGTH);
            MGL.DrawLine(pos1, tmpPos);
        }

        //
        // Draw handler for Scale mode
        //
        private void DrawScaleHandler(Color c, Vector2 pos0, float angle, float length)
        {
            Handles.color = c;

            Vector2 pos1 = new Vector2(m_Pos.x + Mathf.Cos(angle) * length, m_Pos.y + Mathf.Sin(angle) * length);
            MGL.DrawLine(pos0, pos1);

            DrawPoint(c, pos1, DEFAULT_SCALEHANDLER_RADIUS, m_Rot);
        }

        //
        // Draw handler for Rotate mode
        //
        private void DrawRotHandler(Color c, Vector2 pos0, float angle, float length)
        {
            Handles.color = c;

            Vector2 pos1 = new Vector2(m_Pos.x + Mathf.Cos(angle) * length, m_Pos.y + Mathf.Sin(angle) * length);
            MGL.DrawLine(pos0, pos1);
        }

        //
        // Draw Point
        //
        private void DrawPoint(Color c, Vector2 pos, float radius, float angle)
        {
            Handles.color = c;

            Vector2 p0 = new Vector2(pos.x + (Mathf.Cos(angle + HALF_PI * 0.5f) * radius), pos.y + (Mathf.Sin(angle + HALF_PI * 0.5f) * radius));
            Vector2 p1 = new Vector2(pos.x + (Mathf.Cos(angle + HALF_PI * 1.5f) * radius), pos.y + (Mathf.Sin(angle + HALF_PI * 1.5f) * radius));
            Vector2 p2 = new Vector2(pos.x + (Mathf.Cos(angle + HALF_PI * 2.5f) * radius), pos.y + (Mathf.Sin(angle + HALF_PI * 2.5f) * radius));
            Vector2 p3 = new Vector2(pos.x + (Mathf.Cos(angle + HALF_PI * 3.5f) * radius), pos.y + (Mathf.Sin(angle + HALF_PI * 3.5f) * radius));

            MGL.DrawLine(p0, p1);
            MGL.DrawLine(p1, p2);
            MGL.DrawLine(p2, p3);
            MGL.DrawLine(p3, p0);
        }

        //
        // Test all touchables parts of gizmo when is in Move mode
        //
        private bool IsTouchedMove(Vector2 mouseScreenPos)
        {
            // Test center point
            if (TestPointTouch(m_Pos, DEFAULT_POINT_RADIUS, mouseScreenPos))
            {
                m_SelectedPart = E_SelectedPart.E_SP_PIVOT;
                return true;
            }
            // Test X axe
            else if (TestAreaTouch(m_Pos, m_Rot, DEFAULT_AXE_LENGTH, DEFAULT_AXE_TOUCHSIZE, mouseScreenPos))
            {
                m_SelectedPart = E_SelectedPart.E_SP_AXE_X;
                return true;
            }
            // Test Y axe
            else if (TestAreaTouch(m_Pos, m_Rot - HALF_PI, DEFAULT_AXE_LENGTH, DEFAULT_AXE_TOUCHSIZE, mouseScreenPos))
            {
                m_SelectedPart = E_SelectedPart.E_SP_AXE_Y;
                return true;
            }

            return false;
        }

        //
        // Test all touchables parts of gizmo when is in Scale mode
        //
        private bool IsTouchedScale(Vector2 mouseScreenPos)
        {
            // Test center point
            if (TestPointTouch(m_Pos, DEFAULT_POINT_RADIUS, mouseScreenPos))
            {
                m_SelectedPart = E_SelectedPart.E_SP_PIVOT;
                return true;
            }
            // Test X axe
            else if (TestAreaTouch(m_Pos, m_Rot, DEFAULT_AXE_LENGTH, DEFAULT_AXE_TOUCHSIZE, mouseScreenPos))
            {
                m_SelectedPart = E_SelectedPart.E_SP_AXE_X;
                return true;
            }
            // Test Y axe
            else if (TestAreaTouch(m_Pos, m_Rot - HALF_PI, DEFAULT_AXE_LENGTH, DEFAULT_AXE_TOUCHSIZE, mouseScreenPos))
            {
                m_SelectedPart = E_SelectedPart.E_SP_AXE_Y;
                return true;
            }

            return false;
        }

        //
        // Test all touchables parts of gizmo when is in Rotate mode
        //
        private bool IsTouchedRotate(Vector2 mouseScreenPos)
        {
            // Test all 4 lines
            for (int i = 0; i < 4; ++i)
            {
                if (TestAreaTouch(m_Pos, m_Rot - HALF_PI * i, DEFAULT_AXE_LENGTH, DEFAULT_AXE_TOUCHSIZE, mouseScreenPos))
                {
                    m_SelectedPart = E_SelectedPart.E_SP_PIVOT;
                    return true;
                }
            }

            return false;
        }

        //
        // Test all touchables parts of gizmo when is in Size mode
        //
        private bool IsTouchedSize(Vector2 mouseScreenPos)
        {
            return IsTouchedScale(mouseScreenPos);
        }

        //
        // Test if point was touched
        //
        private bool TestPointTouch(Vector2 pos, float touchRadius, Vector2 touchScreenPos)
        {
            //Debug.Log("touch pos = " + touchScreenPos.x + ", " + touchScreenPos.y + ", m_Pos = " + pos.x + ", " + pos.y);

            if ((touchScreenPos.x >= pos.x - touchRadius) &&
                (touchScreenPos.x <= pos.x + touchRadius) &&
                (touchScreenPos.y >= pos.y - touchRadius) &&
                (touchScreenPos.y <= pos.y + touchRadius))
            {
                return true;
            }

            return false;
        }

        //
        // Test if area has been touched
        // Area is defined by one point (pos), length, width and angle.
        //
        private bool TestAreaTouch(Vector2 pos, float angle, float length, float width, Vector2 touchScreenPos)
        {
            Vector2 touchScreenPos2 = new Vector2();
            Vector2 delta = new Vector2();

            delta = touchScreenPos - pos;

            touchScreenPos2.x = delta.x * Mathf.Cos(-angle) - delta.y * Mathf.Sin(-angle);
            touchScreenPos2.y = delta.x * Mathf.Sin(-angle) + delta.y * Mathf.Cos(-angle);

            touchScreenPos2 += pos;

            if ((touchScreenPos2.x >= pos.x - width) &&
                (touchScreenPos2.x <= pos.x + length + width) &&
                (touchScreenPos2.y >= pos.y - width) &&
                (touchScreenPos2.y <= pos.y + width))
            {
                return true;
            }

            return false;
        }
    }
}