using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    /// <summary>
    /// 处理旧输入系统的 UI 输入与 Kmax 空间指针事件。
    /// </summary>
    public class KmaxInputModule : StandaloneInputModule, IKmaxPointerEventHost
    {
        [Space]
        [SerializeField]
        private int stylusDragThreshold = 10;

        private KmaxPointerEventProcessor pointerProcessor;

        /// <inheritdoc/>
        public int StylusDragThreshold
        {
            get => stylusDragThreshold;
            set => stylusDragThreshold = Mathf.Max(0, value);
        }

        EventSystem IKmaxPointerEventHost.EventSystem => eventSystem;

#if UNITY_EDITOR
        /// <inheritdoc/>
        protected override void OnValidate()
        {
            base.OnValidate();
            stylusDragThreshold = Mathf.Max(0, stylusDragThreshold);
        }
#endif

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();
            EnsurePointerProcessor();
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            pointerProcessor?.Clear();
            base.OnDisable();
        }

        /// <inheritdoc/>
        public override void Process()
        {
            OverrideBaseInput();
            base.Process();
            EnsurePointerProcessor().Process(stylusDragThreshold);
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

        private void OverrideBaseInput()
        {
            if (inputOverride == null)
            {
                var current = GetComponent<KmaxBaseInput>();
                inputOverride = current == null
                    ? gameObject.AddComponent<KmaxBaseInput>()
                    : current;
            }
        }
    }
}
