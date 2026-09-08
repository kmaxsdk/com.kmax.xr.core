using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    /// <summary>
    /// 为旧输入系统与新输入系统提供统一的触摸输入访问入口。
    /// </summary>
    internal static class KmaxInputCompatibility
    {
        /// <summary>
        /// 定义可选输入后端提供触摸状态的方式。
        /// </summary>
        internal interface ITouchProvider
        {
            /// <summary>
            /// 获取当前有效触摸数量。
            /// </summary>
            int TouchCount { get; }

            /// <summary>
            /// 获取指定有效触摸的位置。
            /// </summary>
            /// <param name="index">有效触摸索引。</param>
            /// <returns>触摸屏幕坐标。</returns>
            Vector2 GetTouchPosition(int index);

            /// <summary>
            /// 判断指针事件是否来自触摸输入。
            /// </summary>
            /// <param name="eventData">指针事件数据。</param>
            /// <returns>来自触摸输入时返回 true。</returns>
            bool IsTouch(PointerEventData eventData);

            /// <summary>
            /// 判断指针事件是否来自主触摸点。
            /// </summary>
            /// <param name="eventData">指针事件数据。</param>
            /// <returns>来自主触摸点时返回 true。</returns>
            bool IsPrimaryTouch(PointerEventData eventData);
        }

        internal static ITouchProvider TouchProvider { get; set; }

        internal static int GetTouchCount(PointerEventData eventData)
        {
            if (TouchProvider != null)
                return TouchProvider.TouchCount;
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.touchCount;
#else
            return 0;
#endif
        }

        internal static Vector2 GetTouchPosition(PointerEventData eventData, int index)
        {
            if (TouchProvider != null)
                return TouchProvider.GetTouchPosition(index);
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetTouch(index).position;
#else
            return Vector2.zero;
#endif
        }

        internal static bool IsTouch(PointerEventData eventData)
        {
            if (TouchProvider != null)
                return TouchProvider.IsTouch(eventData);
            return eventData.pointerId >= 0;
        }

        internal static bool IsPrimaryTouch(PointerEventData eventData)
        {
            if (TouchProvider != null)
                return TouchProvider.IsPrimaryTouch(eventData);
            return eventData.pointerId == 0;
        }
    }
}
