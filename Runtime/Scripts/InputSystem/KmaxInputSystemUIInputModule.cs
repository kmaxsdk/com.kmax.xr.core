using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace KmaxXR
{
    /// <summary>
    /// 处理新输入系统的 UI 输入与 Kmax 空间指针事件。
    /// </summary>
    [AddComponentMenu("Event/Kmax Input System UI Input Module")]
    public class KmaxInputSystemUIInputModule : InputSystemUIInputModule,
        IKmaxPointerEventHost
    {
        [Header("Kmax Settings")]
        [Tooltip("触笔开始拖拽前允许的最大移动距离（像素）。")]
        [SerializeField]
        private int stylusDragThreshold = 20;

        private readonly List<InputBinding> originalPointOverrides =
            new List<InputBinding>();
        private InputAction processedPointAction;
        private KmaxPointerEventProcessor pointerProcessor;

        /// <inheritdoc/>
        public int StylusDragThreshold
        {
            get => stylusDragThreshold;
            set => stylusDragThreshold = Mathf.Max(0, value);
        }

        EventSystem IKmaxPointerEventHost.EventSystem => eventSystem;

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            KmaxInputSystemRegistration.Register();
            base.OnEnable();
            EnsurePointerProcessor();
            EnsurePositionProcessor();
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            pointerProcessor?.Clear();
            RestorePositionProcessor();
            base.OnDisable();
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void OnValidate()
        {
            base.OnValidate();
            stylusDragThreshold = Mathf.Max(0, stylusDragThreshold);
        }
#endif

        /// <inheritdoc/>
        public override void Process()
        {
            EnsurePositionProcessor();
            var previousProvider = KmaxInputCompatibility.TouchProvider;
            KmaxInputCompatibility.TouchProvider = KmaxInputSystemTouchProvider.Instance;
            try
            {
                base.Process();
                EnsurePointerProcessor().Process(stylusDragThreshold);
            }
            finally
            {
                KmaxInputCompatibility.TouchProvider = previousProvider;
            }
        }

        /// <inheritdoc/>
        public override bool IsPointerOverGameObject(int pointerId)
        {
            return (pointerProcessor != null && pointerProcessor.IsPointerOverGameObject(pointerId)) ||
                base.IsPointerOverGameObject(pointerId);
        }

        void IKmaxPointerEventHost.HandlePointerExitAndEnter(
            PointerEventData eventData, GameObject target)
        {
            HandlePointerExitAndEnter(eventData, target);
        }

        private KmaxPointerEventProcessor EnsurePointerProcessor()
        {
            if (pointerProcessor == null)
                pointerProcessor = new KmaxPointerEventProcessor(this);
            return pointerProcessor;
        }

        internal void EnsurePositionProcessor()
        {
            var action = point?.action;
            if (action == processedPointAction)
                return;

            RestorePositionProcessor();
            if (action == null)
                return;

            processedPointAction = action;
            for (int i = 0; i < action.bindings.Count; ++i)
            {
                var binding = action.bindings[i];
                originalPointOverrides.Add(new InputBinding
                {
                    overridePath = binding.overridePath,
                    overrideInteractions = binding.overrideInteractions,
                    overrideProcessors = binding.overrideProcessors
                });

                string processors = binding.effectiveProcessors;
                if (ContainsPositionProcessor(processors))
                    continue;

                binding.overrideProcessors = string.IsNullOrEmpty(processors)
                    ? KmaxInputSystemRegistration.ProcessorName
                    : processors + "," + KmaxInputSystemRegistration.ProcessorName;
                action.ApplyBindingOverride(i, binding);
            }
        }

        private void RestorePositionProcessor()
        {
            if (processedPointAction == null)
                return;

            int count = Mathf.Min(
                processedPointAction.bindings.Count, originalPointOverrides.Count);
            for (int i = 0; i < count; ++i)
                processedPointAction.ApplyBindingOverride(i, originalPointOverrides[i]);

            originalPointOverrides.Clear();
            processedPointAction = null;
        }

        private static bool ContainsPositionProcessor(string processors)
        {
            return !string.IsNullOrEmpty(processors) &&
                processors.IndexOf(
                    KmaxInputSystemRegistration.ProcessorName,
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
