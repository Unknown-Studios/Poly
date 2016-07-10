using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityStandardAssets.CrossPlatformInput
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public int MovementRange = 100;

        public AxisOption axesToUse = AxisOption.Both;

        // The options for the axes that the still will use
        public string horizontalAxisName = "Horizontal";

        // The name given to the horizontal axis for the cross platform input
        public string verticalAxisName = "Vertical";

        private Vector3 m_StartPos;

        // The name given to the vertical axis for the cross platform input
        private bool m_UseX;

        // Toggle for using the x axis
        private bool m_UseY;

        // Toggle for using the Y axis
        private CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis;

        // Reference to the joystick in the cross platform input
        private CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis;

        public enum AxisOption
        {
            // Options for which axes to use
            Both, // Use both

            OnlyHorizontal, // Only horizontal
            OnlyVertical // Only vertical
        }

        // Reference to the joystick in the cross platform input

        public void OnDrag(PointerEventData data)
        {
            Vector3 newPos = Vector3.zero;

            if (m_UseX)
            {
                int delta = (int)(data.position.x - m_StartPos.x);
                delta = Mathf.Clamp(delta, -MovementRange, MovementRange);
                newPos.x = delta;
            }

            if (m_UseY)
            {
                int delta = (int)(data.position.y - m_StartPos.y);
                delta = Mathf.Clamp(delta, -MovementRange, MovementRange);
                newPos.y = delta;
            }
            transform.position = new Vector3(m_StartPos.x + newPos.x, m_StartPos.y + newPos.y, m_StartPos.z + newPos.z);
            UpdateVirtualAxes(transform.position);
        }

        public void OnPointerUp(PointerEventData data)
        {
            transform.position = m_StartPos;
            UpdateVirtualAxes(m_StartPos);
        }

        public void OnPointerDown(PointerEventData data)
        {
        }

        private void OnEnable()
        {
            CreateVirtualAxes();
        }

        private void Start()
        {
            m_StartPos = transform.position;
        }

        private void UpdateVirtualAxes(Vector3 value)
        {
            var delta = m_StartPos - value;
            delta.y = -delta.y;
            delta /= MovementRange;
            if (m_UseX)
            {
                m_HorizontalVirtualAxis.Update(-delta.x);
            }

            if (m_UseY)
            {
                m_VerticalVirtualAxis.Update(delta.y);
            }
        }

        private void CreateVirtualAxes()
        {
            // set axes to use
            m_UseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
            m_UseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);

            // create new axes based on axes to use
            if (m_UseX)
            {
                m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
                CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
            }
            if (m_UseY)
            {
                m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
                CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
            }
        }

        private void OnDisable()
        {
            // remove the joysticks from the cross platform input
            if (m_UseX)
            {
                m_HorizontalVirtualAxis.Remove();
            }
            if (m_UseY)
            {
                m_VerticalVirtualAxis.Remove();
            }
        }
    }
}