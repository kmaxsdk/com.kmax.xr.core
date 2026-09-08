using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace KmaxXR
{
    /// <summary>
    /// 修正新输入系统指针控件在左右格式立体渲染下的屏幕位置。
    /// </summary>
    public sealed class KmaxInputPositionProcessor : InputProcessor<Vector2>
    {
        /// <inheritdoc/>
        public override Vector2 Process(Vector2 value, InputControl control)
        {
            if (control != null && control.device is Touchscreen)
                return KmaxInputPosition.CorrectTouch(value);
            return KmaxInputPosition.Correct(value);
        }
    }

    /// <summary>
    /// 负责向 Unity 新输入系统注册 Kmax 输入处理器。
    /// </summary>
    internal static class KmaxInputSystemRegistration
    {
        internal const string ProcessorName = "kmaxInputPosition";
        private static bool registered;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Register()
        {
            if (registered)
                return;
            InputSystem.RegisterProcessor<KmaxInputPositionProcessor>(ProcessorName);
            registered = true;
        }
    }
}
