using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace KmaxXR
{
#if UNITY_STANDALONE// || UNITY_EDITOR
    public static partial class KmaxNative
    {
        const string CoreDllName = "kXRCore";
        [DllImport(CoreDllName)]
        internal static extern int kxrGetSupportedVRMode();
        [DllImport(CoreDllName)]
        internal static extern int kxrSetTracking(int s_tracking);
        [DllImport(CoreDllName)]
        internal static extern int kxrSetDisplayMode(int s_display);
        [DllImport(CoreDllName)]
        internal static extern int kxrPenShake(int time, int strength);
        [DllImport(CoreDllName)]
        internal static extern int kxrGetTrackData(ref TrackerData data);

        /// <summary>
        /// 是否采用软件接口立体显示
        /// </summary>
        public static bool UsingStereoscopic => renderMode == RenderMode.Stereoscopic;
        private static int initCode = -1;
        /// <summary>
        /// 初始化并确定立体显示方法
        /// </summary>
        /// <returns>初始化结果</returns>
        internal static int InitializeAndDeterminRenderMode()
        {
            if (initCode >= 0) return initCode;
            // 获取支持的VR模式
            int vrMode = kxrGetSupportedVRMode();
            renderMode = GetFirstFlag<RenderMode>(vrMode);
            if (renderMode == RenderMode.Mono) // 不支持立体显示
            {
                Debug.Log($"Use RenderMode {renderMode}");
                return initCode = 1;
            }

#if UNITY_EDITOR
            // 根据编辑器选项确定是否要开启立体预览
            const string XR_DISPLAY_ENABLE = "EnableStereoDisplay";
            bool enable = UnityEditor.EditorPrefs.GetBool(XR_DISPLAY_ENABLE);
            if (!enable)
            {
                renderMode = RenderMode.Mono;
                return initCode = 1;
            }
#endif

            // 获取当前的渲染API类型
            GraphicsDeviceType currentGraphicsDeviceType = SystemInfo.graphicsDeviceType;
            Debug.Log("Current Graphics Device Type: " + currentGraphicsDeviceType);
            if (currentGraphicsDeviceType != GraphicsDeviceType.Direct3D11)
            {
                renderMode = RenderMode.SideBySide;
                return initCode = 2;
            }

            int width = 32, height = 32;
            var Tex = new Texture2D(width, height);
            initCode = kxrCreateStereoOverlay(System.IntPtr.Zero, Tex.GetNativeTexturePtr(), (int)QualitySettings.activeColorSpace);
            if (initCode == 0)
            {
                renderMode = RenderMode.Stereoscopic;
            }
            else
            {
                renderMode = RenderMode.SideBySide;
            }
            Debug.Log($"Use RenderMode {renderMode}. Return code {initCode}.");
            UnityEngine.Object.Destroy(Tex);
            return initCode;
        }

        internal static int GetDXGIFormatForRenderTextureFormat(RenderTextureFormat format)
        {
            bool srgb = QualitySettings.activeColorSpace == ColorSpace.Linear;
            return format switch
            {
                RenderTextureFormat.ARGB32 => srgb ? 29 : 28,
                RenderTextureFormat.DefaultHDR => 10,
                _ => 0,
            };
        }

        #region Overlay
        [DllImport(CoreDllName)]
        internal extern static int kxrCreateStereoOverlay(System.IntPtr hwnd, System.IntPtr texture, int colorSpace);
        [DllImport(CoreDllName)]
        internal extern static int kxrSetTexture(System.IntPtr texture, System.IntPtr texture2, int texture_format);
        [DllImport(CoreDllName)]
        internal extern static System.IntPtr kxrGetRenderFunc();

        [DllImport(CoreDllName)]
        internal extern static int kxrDestroyOverlay();
        #endregion
    }
#endif
}
