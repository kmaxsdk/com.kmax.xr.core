using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    /// <summary>
    /// 定义 Kmax 指针事件处理器访问输入模块所需的功能。
    /// </summary>
    internal interface IKmaxPointerEventHost
    {
        EventSystem EventSystem { get; }
        void HandlePointerExitAndEnter(PointerEventData eventData, GameObject target);
    }

    /// <summary>
    /// 将 Kmax 空间指针状态转换为 Unity 指针事件。
    /// </summary>
    internal sealed class KmaxPointerEventProcessor
    {
        private const float DoubleClickTime = 0.3f;

        private readonly IKmaxPointerEventHost host;
        private readonly Dictionary<int, PointerEventData> pointerData =
            new Dictionary<int, PointerEventData>();

        internal KmaxPointerEventProcessor(IKmaxPointerEventHost host)
        {
            this.host = host;
        }

        internal bool Process(int dragThreshold)
        {
            foreach (var pointer in KmaxPointer.Pointers)
            {
                var left = GetEventData(pointer, PointerEventData.InputButton.Left);
                ProcessPress(left, pointer.StateOf(PointerEventData.InputButton.Left));
                host.HandlePointerExitAndEnter(left, left.pointerCurrentRaycast.gameObject);
                ProcessDrag(left, dragThreshold);

                var right = GetEventData(pointer, PointerEventData.InputButton.Right, left);
                ProcessPress(right, pointer.StateOf(PointerEventData.InputButton.Right));
                ProcessDrag(right, dragThreshold);

                var middle = GetEventData(pointer, PointerEventData.InputButton.Middle, left);
                ProcessPress(middle, pointer.StateOf(PointerEventData.InputButton.Middle));
                ProcessDrag(middle, dragThreshold);
            }

            return KmaxPointer.Enable;
        }

        internal bool IsPointerOverGameObject(int pointerId)
        {
            PointerEventData data;
            return pointerData.TryGetValue(pointerId, out data) && data.pointerEnter != null;
        }

        internal void Clear()
        {
            foreach (var data in pointerData.Values)
            {
                host.HandlePointerExitAndEnter(data, null);
                if (data.pointerPress != null)
                    ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);
                if (data.pointerDrag != null && data.dragging)
                    ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.endDragHandler);
            }
            pointerData.Clear();
        }

        private PointerEventData GetEventData(
            KmaxPointer pointer,
            PointerEventData.InputButton button,
            PointerEventData source = null)
        {
            int pointerId = pointer.Id + (int)button;
            PointerEventData data;
            if (!pointerData.TryGetValue(pointerId, out data))
            {
                data = new PointerEventData(host.EventSystem)
                {
                    pointerId = pointerId,
                    position = pointer.ScreenPosition
                };
                pointerData.Add(pointerId, data);
            }

            data.Reset();
            data.button = button;
            if (source != null)
            {
                CopyFromTo(source, data);
            }
            else
            {
                pointer.UpdateState();
                pointer.Raycast(data);
            }
            return data;
        }

        private static void CopyFromTo(PointerEventData source, PointerEventData target)
        {
            target.position = source.position;
            target.delta = source.delta;
            target.scrollDelta = source.scrollDelta;
            target.pointerCurrentRaycast = source.pointerCurrentRaycast;
            target.pointerEnter = source.pointerEnter;
        }

        private void ProcessPress(
            PointerEventData data,
            PointerEventData.FramePressState state)
        {
            var currentObject = data.pointerCurrentRaycast.gameObject;
            if (state == PointerEventData.FramePressState.Pressed)
            {
                data.eligibleForClick = true;
                data.delta = Vector2.zero;
                data.dragging = false;
                data.useDragThreshold = true;
                data.pressPosition = data.position;
                data.pointerPressRaycast = data.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentObject, data);
                var pressed = ExecuteEvents.ExecuteHierarchy(
                    currentObject, data, ExecuteEvents.pointerDownHandler);
                if (pressed == null)
                    pressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);

                float time = Time.unscaledTime;
                if (pressed == data.lastPress && time - data.clickTime < DoubleClickTime)
                    ++data.clickCount;
                else
                    data.clickCount = 1;

                data.clickTime = time;
                data.pointerPress = pressed;
                data.rawPointerPress = currentObject;
                data.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentObject);
                if (data.pointerDrag != null)
                    ExecuteEvents.Execute(
                        data.pointerDrag, data, ExecuteEvents.initializePotentialDrag);
            }
            else if (state == PointerEventData.FramePressState.Released)
            {
                ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);
                var clickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);
                if (data.pointerPress == clickHandler && data.eligibleForClick)
                    ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerClickHandler);
                else if (data.pointerDrag != null && data.dragging)
                    ExecuteEvents.ExecuteHierarchy(currentObject, data, ExecuteEvents.dropHandler);

                data.eligibleForClick = false;
                data.pointerPress = null;
                data.rawPointerPress = null;
                if (data.pointerDrag != null && data.dragging)
                    ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.endDragHandler);
                data.dragging = false;
                data.pointerDrag = null;

                if (currentObject != data.pointerEnter)
                {
                    host.HandlePointerExitAndEnter(data, null);
                    host.HandlePointerExitAndEnter(data, currentObject);
                }
            }
        }

        private void DeselectIfSelectionChanged(GameObject currentObject, BaseEventData eventData)
        {
            var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(currentObject);
            if (selected != host.EventSystem.currentSelectedGameObject)
                host.EventSystem.SetSelectedGameObject(null, eventData);
        }

        private static bool ShouldStartDrag(
            Vector2 pressPosition,
            Vector2 currentPosition,
            float threshold,
            bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;
            return (pressPosition - currentPosition).sqrMagnitude >= threshold * threshold;
        }

        private static void ProcessDrag(PointerEventData data, int dragThreshold)
        {
            if (!data.IsPointerMoving() ||
                Cursor.lockState == CursorLockMode.Locked ||
                data.pointerDrag == null)
                return;

            if (!data.dragging && ShouldStartDrag(
                    data.pressPosition, data.position, dragThreshold, data.useDragThreshold))
            {
                ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.beginDragHandler);
                data.dragging = true;
            }

            if (!data.dragging)
                return;

            if (data.pointerPress != data.pointerDrag)
            {
                ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);
                data.eligibleForClick = false;
                data.pointerPress = null;
                data.rawPointerPress = null;
            }
            ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.dragHandler);
        }
    }
}
