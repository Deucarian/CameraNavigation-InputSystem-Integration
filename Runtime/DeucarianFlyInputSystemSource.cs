using Deucarian.CameraNavigation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Deucarian.CameraNavigation.InputSystemIntegration
{
    [DisallowMultipleComponent]
    public sealed class DeucarianFlyInputSystemSource : MonoBehaviour
    {
        [SerializeField] private DeucarianInputSystemNavigationSettings settings;
        [Tooltip("Optional MonoBehaviour implementing IDeucarianNavigationInputBlocker.")]
        [SerializeField] private MonoBehaviour inputBlocker;

        private bool lookDragBlocked;

        public DeucarianInputSystemNavigationSettings Settings
        {
            get => settings;
            set => settings = value;
        }

        public void SetInputBlocker(MonoBehaviour blocker)
        {
            inputBlocker = blocker;
        }

        public DeucarianFlyCameraInput ReadInput()
        {
            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;
            Vector2 pointerPosition =
                mouse != null ? mouse.position.ReadValue() : Vector2.zero;
            bool pointerBlocked = IsPointerBlocked(pointerPosition);
            bool keyboardBlocked = IsKeyboardBlocked();
            ButtonControl lookButton =
                DeucarianInputSystemDeviceUtility.GetMouseButton(
                    mouse,
                    settings != null
                        ? settings.FlyLookButton
                        : DeucarianMouseButton.Right);

            if (lookButton != null && lookButton.wasPressedThisFrame)
            {
                lookDragBlocked = pointerBlocked;
            }

            if (lookButton != null && lookButton.wasReleasedThisFrame)
            {
                lookDragBlocked = false;
            }

            Vector2 look =
                mouse != null &&
                lookButton != null &&
                lookButton.isPressed &&
                !lookDragBlocked &&
                !pointerBlocked
                    ? mouse.delta.ReadValue()
                    : Vector2.zero;
            float zoom = mouse != null && !pointerBlocked
                ? DeucarianInputSystemDeviceUtility.NormalizeScroll(
                    mouse.scroll.ReadValue().y,
                    settings)
                : 0f;
            Vector3 move = keyboardBlocked
                ? Vector3.zero
                : DeucarianInputSystemDeviceUtility.ReadMovement(keyboard, settings);
            bool boost = !keyboardBlocked &&
                         DeucarianInputSystemDeviceUtility.ReadBoost(keyboard, settings);
            bool slow = !keyboardBlocked &&
                        DeucarianInputSystemDeviceUtility.ReadSlow(keyboard, settings);

            return new DeucarianFlyCameraInput(look, move, zoom, boost, slow);
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
    }
}
