using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace KmaxXR
{
    /// <summary>
    /// 从 Unity Input System 读取触摸状态。
    /// </summary>
    internal sealed class KmaxInputSystemTouchProvider : KmaxInputCompatibility.ITouchProvider
    {
        internal static readonly KmaxInputSystemTouchProvider Instance =
            new KmaxInputSystemTouchProvider();

        /// <inheritdoc/>
        public int TouchCount
        {
            get
            {
                var touchscreen = Touchscreen.current;
                if (touchscreen == null)
                    return 0;

                int count = 0;
                foreach (var touch in touchscreen.touches)
                {
                    if (touch.press.isPressed)
                        ++count;
                }
                return count;
            }
        }

        /// <inheritdoc/>
        public Vector2 GetTouchPosition(int index)
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return Vector2.zero;

            int activeIndex = 0;
            foreach (var touch in touchscreen.touches)
            {
                if (!touch.press.isPressed)
                    continue;
                if (activeIndex == index)
                    return KmaxInputPosition.CorrectTouch(touch.position.ReadValue());
                ++activeIndex;
            }
            return Vector2.zero;
        }

        /// <inheritdoc/>
        public bool IsTouch(PointerEventData eventData)
        {
            return eventData is ExtendedPointerEventData extended &&
                extended.pointerType == UIPointerType.Touch;
        }

        /// <inheritdoc/>
        public bool IsPrimaryTouch(PointerEventData eventData)
        {
            var extended = eventData as ExtendedPointerEventData;
            var touchscreen = extended?.device as Touchscreen;
            return touchscreen != null &&
                extended.pointerType == UIPointerType.Touch &&
                extended.touchId == touchscreen.primaryTouch.touchId.ReadValue();
        }
    }
}
