using KmaxXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 显示设备信息，并处理示例场景的返回与退出操作。
/// </summary>
public class AppQuit : MonoBehaviour
{
    [SerializeField]
    Text title;
    void Start()
    {
        if (title != null)
        {
            title.text += $"\nSDK Version: {KmaxNative.SDKVersion}\n{KmaxNative.DeviceId}";
            title.text += $"\nDevice Model: {KmaxNative.DeviceModel}";
        }
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        bool escapePressed = Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        bool escapePressed = Input.GetKeyDown(KeyCode.Escape);
#else
        bool escapePressed = false;
#endif
        if (escapePressed)
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.buildIndex != 0)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                Application.Quit();
            }
        }
    }

    /// <summary>
    /// 退出示例应用程序。
    /// </summary>
    public void OnQuitButton()
    {
        Application.Quit();
    }

    /// <summary>
    /// 按名称加载示例场景。
    /// </summary>
    /// <param name="name">场景名称。</param>
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    /// <summary>
    /// 按构建索引加载示例场景。
    /// </summary>
    /// <param name="index">场景构建索引。</param>
    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
