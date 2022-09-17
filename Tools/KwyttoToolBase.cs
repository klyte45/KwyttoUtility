using System;
using System.Diagnostics;
using UnityEngine;

namespace Kwytto.Tools
{
    public abstract class KwyttoToolBase : ToolBase
    {
        protected Vector3 m_raycastHit;

        private long m_rightClickTime;

        private long m_leftClickTime;

        public event Action<bool> EventEnableChanged;

        protected override void OnEnable()
        {
            base.OnEnable();
            EventEnableChanged?.Invoke(true);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            EventEnableChanged?.Invoke(false);
        }

        protected override void OnToolUpdate()
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                ToolsModifierControl.SetTool<DefaultTool>();
                Event.current.Use();
            }
            var isInsideUI = m_toolController.IsInsideUI;
            if (m_leftClickTime == 0L && Input.GetMouseButton(0) && !isInsideUI)
            {
                m_leftClickTime = Stopwatch.GetTimestamp();
                OnLeftMouseDown();
            }
            if (m_leftClickTime != 0L)
            {
                var num = ElapsedMilliseconds(m_leftClickTime);
                if (!Input.GetMouseButton(0))
                {
                    m_leftClickTime = 0L;
                    if (num < 200L)
                    {
                        OnLeftClick();
                    }
                    else
                    {
                        OnLeftDragStop();
                    }
                    OnLeftMouseUp();
                }
                else if (num >= 200L)
                {
                    OnLeftDrag();
                }
            }
            if (m_rightClickTime == 0L && Input.GetMouseButton(1) && !isInsideUI)
            {
                m_rightClickTime = Stopwatch.GetTimestamp();
                OnRightMouseDown();
            }
            if (m_rightClickTime != 0L)
            {
                var num2 = ElapsedMilliseconds(m_rightClickTime);
                if (!Input.GetMouseButton(1))
                {
                    m_rightClickTime = 0L;
                    if (num2 < 200L)
                    {
                        OnRightClick();
                    }
                    else
                    {
                        OnRightDragStop();
                    }
                    OnRightMouseUp();
                }
                else if (num2 >= 200L)
                {
                    OnRightDrag();
                }
            }
            if (!isInsideUI && Cursor.visible)
            {
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                OnRaycastHoverInstance(mouseRay);
            }
        }

        protected abstract void OnRaycastHoverInstance(Ray mouseRay);

        protected virtual void OnRightDrag() { }
        protected virtual void OnRightMouseUp() { }
        protected virtual void OnRightDragStop() { }
        protected virtual void OnRightClick() => ToolsModifierControl.SetTool<DefaultTool>();
        protected virtual void OnRightMouseDown() { }
        protected virtual void OnLeftDrag() { }
        protected virtual void OnLeftMouseUp() { }
        protected virtual void OnLeftDragStop() { }
        protected virtual void OnLeftClick() { }
        protected virtual void OnLeftMouseDown() { }
        private long ElapsedMilliseconds(long startTime)
        {
            var timestamp = Stopwatch.GetTimestamp();
            long num;
            if (timestamp > startTime)
            {
                num = timestamp - startTime;
            }
            else
            {
                num = startTime - timestamp;
            }
            return num / (Stopwatch.Frequency / 1000L);
        }
    }
}