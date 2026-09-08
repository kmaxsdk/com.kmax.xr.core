using UnityEngine;

namespace KmaxXR
{
    /// <summary>
    /// 提供左右格式立体渲染所需的输入坐标修正功能。
    /// </summary>
    public static class KmaxInputPosition
    {
        private const int SplitCount = 2;

        /// <summary>
        /// 获取是否修正左右格式立体渲染下的输入坐标。
        /// </summary>
        public static bool Enabled { get; internal set; }

        internal static Vector2 Correct(Vector2 position)
        {
            if (!Enabled || Screen.width <= 0)
                return position;

            float fragmentWidth = (float)Screen.width / SplitCount;
            position.x = position.x % fragmentWidth * SplitCount;
            return position;
        }

        internal static Vector2 CorrectTouch(Vector2 position)
        {
            if (Screen.height <= 0 || (float)Screen.width / Screen.height < SplitCount)
                return position;
            return Correct(position);
        }
    }
}
