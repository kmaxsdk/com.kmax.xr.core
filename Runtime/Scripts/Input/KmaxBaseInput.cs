using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    /// <summary>
    /// 修正旧输入系统在左右格式立体渲染下的指针位置。
    /// </summary>
    public class KmaxBaseInput : BaseInput
    {
        /// <summary>
        /// 鼠标位置
        /// override mouse position
        /// </summary>
        public override Vector2 mousePosition
        {
            get
            {
                return KmaxInputPosition.Correct(base.mousePosition);
            }
        }

        /// <summary>
        /// 触摸信息
        /// override touch position
        /// </summary>
        /// <param name="index">手指id</param>
        /// <returns>触摸信息</returns>
        public override Touch GetTouch(int index)
        {
            var touch = base.GetTouch(index);
            touch.position = KmaxInputPosition.CorrectTouch(touch.position);
            return touch;
        }
    }
}
