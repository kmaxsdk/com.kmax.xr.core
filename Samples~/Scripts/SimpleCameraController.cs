#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine;

namespace KmaxXR.Demo
{
    /// <summary>
    /// 演示如何通过 Unity 的两套输入后端与 Kmax 触笔控制相机移动。
    /// </summary>
    public class SimpleCameraController : MonoBehaviour
    {
        /// <summary>
        /// 保存相机位置与旋转的插值状态。
        /// </summary>
        private class CameraState
        {
            public float yaw;
            public float pitch;
            public float roll;
            public float x;
            public float y;
            public float z;

            public void SetFromTransform(Transform target)
            {
                pitch = target.eulerAngles.x;
                yaw = target.eulerAngles.y;
                roll = target.eulerAngles.z;
                x = target.position.x;
                y = target.position.y;
                z = target.position.z;
            }

            public void Translate(Vector3 translation)
            {
                Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;
                x += rotatedTranslation.x;
                y += rotatedTranslation.y;
                z += rotatedTranslation.z;
            }

            public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
            {
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
                roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);
                x = Mathf.Lerp(x, target.x, positionLerpPct);
                y = Mathf.Lerp(y, target.y, positionLerpPct);
                z = Mathf.Lerp(z, target.z, positionLerpPct);
            }

            public void UpdateTransform(Transform target)
            {
                target.eulerAngles = new Vector3(pitch, yaw, roll);
                target.position = new Vector3(x, y, z);
            }
        }

        private readonly CameraState targetCameraState = new CameraState();
        private readonly CameraState interpolatingCameraState = new CameraState();

        [Header("Movement Settings")]
        [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
        public float boost = 3.5f;

        [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
        public float positionLerpTime = 0.2f;

        [Header("Rotation Settings")]
        [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
        public AnimationCurve mouseSensitivityCurve = new AnimationCurve(
            new Keyframe(0f, 0.5f, 0f, 5f),
            new Keyframe(1f, 2.5f, 0f, 0f));

        [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
        public float rotationLerpTime = 0.01f;

        [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
        public bool invertY;

        private void OnEnable()
        {
            targetCameraState.SetFromTransform(transform);
            interpolatingCameraState.SetFromTransform(transform);
        }

        private Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = Vector3.zero;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return direction;
            if (keyboard.wKey.isPressed) direction += Vector3.forward;
            if (keyboard.sKey.isPressed) direction += Vector3.back;
            if (keyboard.aKey.isPressed) direction += Vector3.left;
            if (keyboard.dKey.isPressed) direction += Vector3.right;
            if (keyboard.qKey.isPressed) direction += Vector3.down;
            if (keyboard.eKey.isPressed) direction += Vector3.up;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.W)) direction += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) direction += Vector3.back;
            if (Input.GetKey(KeyCode.A)) direction += Vector3.left;
            if (Input.GetKey(KeyCode.D)) direction += Vector3.right;
            if (Input.GetKey(KeyCode.Q)) direction += Vector3.down;
            if (Input.GetKey(KeyCode.E)) direction += Vector3.up;
#endif
            return direction;
        }

        private void Update()
        {
            Vector3 translation = Vector3.zero;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard != null && keyboard.escapeKey.isPressed)
                ExitSample();

            if (mouse != null && mouse.rightButton.wasPressedThisFrame)
                Cursor.lockState = CursorLockMode.Locked;
            if (mouse != null && mouse.rightButton.wasReleasedThisFrame)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            if (mouse != null && mouse.rightButton.isPressed)
                RotateCamera(mouse.delta.ReadValue());
            else
                RotateWithStylus();

            translation = GetInputTranslationDirection() * Time.deltaTime;
            if (keyboard != null && keyboard.leftShiftKey.isPressed)
                translation *= 10f;
            if (mouse != null)
                boost += mouse.scroll.ReadValue().y / 600f;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.Escape))
                ExitSample();

            if (Input.GetMouseButtonDown(1))
                Cursor.lockState = CursorLockMode.Locked;
            if (Input.GetMouseButtonUp(1))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            if (Input.GetMouseButton(1))
                RotateCamera(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
            else
                RotateWithStylus();

            translation = GetInputTranslationDirection() * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftShift))
                translation *= 10f;
            boost += Input.mouseScrollDelta.y * 0.2f;
#endif
            translation *= Mathf.Pow(2f, boost);
            targetCameraState.Translate(translation);

            float positionLerpPct = 1f - Mathf.Exp(
                Mathf.Log(1f - 0.99f) / positionLerpTime * Time.deltaTime);
            float rotationLerpPct = 1f - Mathf.Exp(
                Mathf.Log(1f - 0.99f) / rotationLerpTime * Time.deltaTime);
            interpolatingCameraState.LerpTowards(
                targetCameraState, positionLerpPct, rotationLerpPct);
            interpolatingCameraState.UpdateTransform(transform);
        }

        private void RotateCamera(Vector2 movement)
        {
            movement.y *= invertY ? 1 : -1;
            float sensitivity = mouseSensitivityCurve.Evaluate(movement.magnitude);
            targetCameraState.yaw += movement.x * sensitivity;
            targetCameraState.pitch += movement.y * sensitivity;
        }

        private void RotateWithStylus()
        {
            if (!KmaxPointer.GetPointerButton(KmaxStylus.UniqueId, 1))
                return;
            RotateCamera(KmaxPointer.GetPointerAxis(KmaxStylus.UniqueId));
        }

        private static void ExitSample()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
