using UnityEngine;
using UnityEngine.InputSystem;

namespace Deucarian.CameraNavigation.InputSystemIntegration
{
    public enum DeucarianMouseButton
    {
        Left,
        Right,
        Middle,
        Forward,
        Back
    }

    [CreateAssetMenu(
        fileName = "DeucarianInputSystemNavigationSettings",
        menuName = "Deucarian/Camera Navigation/Input System Settings")]
    public sealed class DeucarianInputSystemNavigationSettings : ScriptableObject
    {
        public const float DefaultOrbitDragThreshold = 25f;
        public const float DefaultPointerDeltaScale = 0.1f;
        public const float DefaultScrollNormalization = 120f;

        [Header("Mouse")]
        [SerializeField] private DeucarianMouseButton orbitRotateButton =
            DeucarianMouseButton.Left;
        [SerializeField] private DeucarianMouseButton orbitPanButton =
            DeucarianMouseButton.Right;
        [SerializeField] private DeucarianMouseButton orbitPivotButton =
            DeucarianMouseButton.Middle;
        [SerializeField] private DeucarianMouseButton flyLookButton =
            DeucarianMouseButton.Right;
        [SerializeField, Min(0f)] private float orbitDragThreshold =
            DefaultOrbitDragThreshold;
        [Tooltip("Scales raw Input System pointer pixels before navigation consumes them.")]
        [SerializeField, Min(0f)] private float pointerDeltaScale =
            DefaultPointerDeltaScale;
        [SerializeField, Min(0.0001f)] private float scrollNormalization =
            DefaultScrollNormalization;

        [Header("Movement")]
        [SerializeField] private Key moveForward = Key.W;
        [SerializeField] private Key moveForwardAlternative = Key.UpArrow;
        [SerializeField] private Key moveBackward = Key.S;
        [SerializeField] private Key moveBackwardAlternative = Key.DownArrow;
        [SerializeField] private Key moveRight = Key.D;
        [SerializeField] private Key moveRightAlternative = Key.RightArrow;
        [SerializeField] private Key moveLeft = Key.A;
        [SerializeField] private Key moveLeftAlternative = Key.LeftArrow;
        [SerializeField] private Key moveUp = Key.E;
        [SerializeField] private Key moveUpAlternative = Key.PageUp;
        [SerializeField] private Key moveDown = Key.Q;
        [SerializeField] private Key moveDownAlternative = Key.PageDown;

        [Header("Modifiers")]
        [SerializeField] private Key boost = Key.LeftShift;
        [SerializeField] private Key boostAlternative = Key.RightShift;
        [SerializeField] private Key slow = Key.LeftCtrl;
        [SerializeField] private Key slowAlternative = Key.RightCtrl;

        public DeucarianMouseButton OrbitRotateButton
        {
            get => orbitRotateButton;
            set => orbitRotateButton = value;
        }

        public DeucarianMouseButton OrbitPanButton
        {
            get => orbitPanButton;
            set => orbitPanButton = value;
        }

        public DeucarianMouseButton OrbitPivotButton
        {
            get => orbitPivotButton;
            set => orbitPivotButton = value;
        }

        public DeucarianMouseButton FlyLookButton
        {
            get => flyLookButton;
            set => flyLookButton = value;
        }

        public float OrbitDragThreshold
        {
            get => Mathf.Max(0f, orbitDragThreshold);
            set => orbitDragThreshold = Mathf.Max(0f, value);
        }

        public float ScrollNormalization
        {
            get => Mathf.Max(0.0001f, scrollNormalization);
            set => scrollNormalization = Mathf.Max(0.0001f, value);
        }

        public float PointerDeltaScale
        {
            get => Mathf.Max(0f, pointerDeltaScale);
            set => pointerDeltaScale = Mathf.Max(0f, value);
        }

        public Key MoveForward => moveForward;
        public Key MoveForwardAlternative => moveForwardAlternative;
        public Key MoveBackward => moveBackward;
        public Key MoveBackwardAlternative => moveBackwardAlternative;
        public Key MoveRight => moveRight;
        public Key MoveRightAlternative => moveRightAlternative;
        public Key MoveLeft => moveLeft;
        public Key MoveLeftAlternative => moveLeftAlternative;
        public Key MoveUp => moveUp;
        public Key MoveUpAlternative => moveUpAlternative;
        public Key MoveDown => moveDown;
        public Key MoveDownAlternative => moveDownAlternative;
        public Key Boost => boost;
        public Key BoostAlternative => boostAlternative;
        public Key Slow => slow;
        public Key SlowAlternative => slowAlternative;

        private void OnValidate()
        {
            orbitDragThreshold = Mathf.Max(0f, orbitDragThreshold);
            pointerDeltaScale = Mathf.Max(0f, pointerDeltaScale);
            scrollNormalization = Mathf.Max(0.0001f, scrollNormalization);
        }
    }
}
