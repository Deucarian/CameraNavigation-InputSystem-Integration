using Deucarian.CameraNavigation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Deucarian.CameraNavigation.InputSystemIntegration
{
    [DisallowMultipleComponent]
    public sealed class DeucarianOrbitInputSystemSource : MonoBehaviour
    {
        [SerializeField] private DeucarianInputSystemNavigationSettings settings;
        [Tooltip("Optional MonoBehaviour implementing IDeucarianNavigationInputBlocker.")]
        [SerializeField] private MonoBehaviour inputBlocker;

        private bool rotateDragBlocked;
        private bool panDragBlocked;
        private bool rotateDragStarted;
        private Vector2 rotateStartPosition;

        public DeucarianInputSystemNavigationSettings Settings
        {
            get => settings;
            set => settings = value;
        }

        public void SetInputBlocker(MonoBehaviour blocker)
        {
            inputBlocker = blocker;
        }

        public DeucarianOrbitInputSystemFrame ReadFrame()
        {
            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;
            Vector2 pointerPosition =
                mouse != null ? mouse.position.ReadValue() : Vector2.zero;
            bool pointerBlocked = IsPointerBlocked(pointerPosition);
            bool keyboardBlocked = IsKeyboardBlocked();

            Vector2 rotate = ReadRotate(mouse, pointerPosition, pointerBlocked);
            Vector2 pan = ReadPan(mouse, pointerBlocked);
            float zoom = ReadZoom(mouse, pointerBlocked);
            Vector3 move = keyboardBlocked
                ? Vector3.zero
                : DeucarianInputSystemDeviceUtility.ReadMovement(keyboard, settings);
            bool boost = !keyboardBlocked &&
                         DeucarianInputSystemDeviceUtility.ReadBoost(keyboard, settings);
            bool slow = !keyboardBlocked &&
                        DeucarianInputSystemDeviceUtility.ReadSlow(keyboard, settings);
            bool pivotRequested =
                !pointerBlocked &&
                WasPressed(
                    DeucarianInputSystemDeviceUtility.GetMouseButton(
                        mouse,
                        GetPivotButton()));

            return new DeucarianOrbitInputSystemFrame(
                new DeucarianOrbitCameraInput(move, rotate, pan, zoom, boost, slow),
                pivotRequested,
                pointerPosition);
        }

        private Vector2 ReadRotate(
            Mouse mouse,
            Vector2 pointerPosition,
            bool pointerBlocked)
        {
            ButtonControl button =
                DeucarianInputSystemDeviceUtility.GetMouseButton(
                    mouse,
                    GetRotateButton());
            if (WasPressed(button))
            {
                rotateStartPosition = pointerPosition;
                rotateDragBlocked = pointerBlocked;
                rotateDragStarted = false;
            }

            if (WasReleased(button))
            {
                rotateDragBlocked = false;
                rotateDragStarted = false;
                return Vector2.zero;
            }

            if (!IsPressed(button) || rotateDragBlocked || pointerBlocked)
            {
                return Vector2.zero;
            }

            float threshold = settings != null
                ? settings.OrbitDragThreshold
                : DeucarianInputSystemNavigationSettings.DefaultOrbitDragThreshold;
            if (!rotateDragStarted)
            {
                rotateDragStarted =
                    Vector2.Distance(rotateStartPosition, pointerPosition) >= threshold;
            }

            return rotateDragStarted && mouse != null
                ? DeucarianInputSystemDeviceUtility.NormalizePointerDelta(
                    mouse.delta.ReadValue(),
                    settings)
                : Vector2.zero;
        }

        private Vector2 ReadPan(Mouse mouse, bool pointerBlocked)
        {
            ButtonControl button =
                DeucarianInputSystemDeviceUtility.GetMouseButton(mouse, GetPanButton());
            if (WasPressed(button))
            {
                panDragBlocked = pointerBlocked;
            }

            if (WasReleased(button))
            {
                panDragBlocked = false;
                return Vector2.zero;
            }

            return mouse != null &&
                   IsPressed(button) &&
                   !panDragBlocked &&
                   !pointerBlocked
                ? DeucarianInputSystemDeviceUtility.NormalizePointerDelta(
                    mouse.delta.ReadValue(),
                    settings)
                : Vector2.zero;
        }

        private float ReadZoom(Mouse mouse, bool pointerBlocked)
        {
            if (mouse == null || pointerBlocked)
            {
                return 0f;
            }

            return DeucarianInputSystemDeviceUtility.NormalizeScroll(
                mouse.scroll.ReadValue().y,
                settings);
        }

        private bool IsPointerBlocked(Vector2 position)
        {
            return inputBlocker is IDeucarianNavigationInputBlocker blocker &&
                   blocker.IsPointerInputBlocked(position);
        }

        private bool IsKeyboardBlocked()
        {
            return inputBlocker is IDeucarianNavigationInputBlocker blocker &&
                   blocker.IsKeyboardInputBlocked();
        }

        private DeucarianMouseButton GetRotateButton() =>
            settings != null ? settings.OrbitRotateButton : DeucarianMouseButton.Left;
        private DeucarianMouseButton GetPanButton() =>
            settings != null ? settings.OrbitPanButton : DeucarianMouseButton.Right;
        private DeucarianMouseButton GetPivotButton() =>
            settings != null ? settings.OrbitPivotButton : DeucarianMouseButton.Middle;

        private static bool IsPressed(ButtonControl button) =>
            button != null && button.isPressed;
        private static bool WasPressed(ButtonControl button) =>
            button != null && button.wasPressedThisFrame;
        private static bool WasReleased(ButtonControl button) =>
            button != null && button.wasReleasedThisFrame;
    }
}
