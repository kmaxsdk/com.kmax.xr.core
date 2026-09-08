using System;
using UnityEditor;
using UnityEngine;

namespace KmaxXR
{
    /// <summary>
    /// 在检查器中显示 Kmax 输入模块的配置和运行状态。
    /// </summary>
    [CustomEditor(typeof(KmaxInputModule))]
    public class KmaxInputModuleEditor : Editor
    {
        private Type inputSystemModuleType;

        private void OnEnable()
        {
            inputSystemModuleType = KmaxMenu.GetInputSystemModuleType();
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            if (inputSystemModuleType != null)
            {
                EditorGUILayout.HelpBox(
                    "检测到 Unity Input System，可将当前模块迁移到新输入系统。",
                    MessageType.Info);
                if (GUILayout.Button("替换为 Kmax Input System UI Input Module"))
                {
                    MigrateToInputSystem();
                    return;
                }
                GUILayout.Space(10);
            }

            base.OnInspectorGUI();

            if (!Application.isPlaying)
                return;

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.Toggle("坐标修正", KmaxInputPosition.Enabled);
        }

        /// <inheritdoc/>
        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        private void MigrateToInputSystem()
        {
            var inputModule = (KmaxInputModule)target;
            var gameObject = inputModule.gameObject;
            int dragThreshold = inputModule.StylusDragThreshold;

            Undo.DestroyObjectImmediate(inputModule);
            var replacement = Undo.AddComponent(gameObject, inputSystemModuleType);
            var serializedReplacement = new SerializedObject(replacement);
            var thresholdProperty = serializedReplacement.FindProperty(
                "stylusDragThreshold");
            if (thresholdProperty != null)
            {
                thresholdProperty.intValue = dragThreshold;
                serializedReplacement.ApplyModifiedProperties();
            }
        }
    }
}
