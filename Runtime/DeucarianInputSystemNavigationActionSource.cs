using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Deucarian.CameraNavigation.InputSystemIntegration
{
    [DisallowMultipleComponent]
    public sealed class DeucarianInputSystemNavigationActionSource :
        MonoBehaviour,
        IDeucarianNavigationActionStateSource
    {
        [SerializeField] private DeucarianInputSystemNavigationSettings settings;

        public DeucarianInputSystemNavigationSettings Settings
        {
            get => settings;
            set => settings = value;
        }

        public DeucarianNavigationActionState ReadActionState(
            DeucarianInputSystemNavigationMode mode,
            bool isTopDown)
        {
            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;
            Vector2 pointerPosition =
                mouse != null ? mouse.position.ReadValue() : Vector2.zero;

            bool pointerStarted = TryGetPointerAction(
                mouse,
                mode,
                isTopDown,
                out bool captureRequested,
                out DeucarianMouseButton captureButton);
            bool keyboardStarted = AnyConfiguredKeyWasPressed(keyboard);
            DeucarianNavigationActionKinds startedActions =
                (pointerStarted
                    ? DeucarianNavigationActionKinds.Pointer
                    : DeucarianNavigationActionKinds.None) |
                (keyboardStarted
                    ? DeucarianNavigationActionKinds.Keyboard
                    : DeucarianNavigationActionKinds.None);

            return new DeucarianNavigationActionState(
                pointerPosition,
                startedActions,
                IsInputNeutral(mouse, keyboard, mode),
                keyboard != null && keyboard.escapeKey.wasPressedThisFrame,
                captureRequested,
                captureButton);
        }

        public bool IsButtonPressed(DeucarianMouseButton button)
        {
            ButtonControl control =
                DeucarianInputSystemDeviceUtility.GetMouseButton(Mouse.current, button);
            return control != null && control.isPressed;
        }

        public bool IsOrbitRotatePressed()
        {
            return IsButtonPressed(GetOrbitRotateButton());
        }

        private bool TryGetPointerAction(
            Mouse mouse,
            DeucarianInputSystemNavigationMode mode,
            bool isTopDown,
            out bool captureRequested,
            out DeucarianMouseButton captureButton)
        {
            captureRequested = false;
            captureButton = default;
            if (mouse == null)
            {
                return false;
            }

            bool scrollStarted = Mathf.Abs(mouse.scroll.ReadValue().y) > 0.0001f;
            if (mode == DeucarianInputSystemNavigationMode.Fly)
            {
                DeucarianMouseButton lookButton = GetFlyLookButton();
                bool lookStarted = WasPressed(mouse, lookButton);
                captureRequested = lookStarted;
                captureButton = lookButton;
                return lookStarted || scrollStarted;
            }

            DeucarianMouseButton panButton = GetOrbitPanButton();
            bool panStarted = WasPressed(mouse, panButton);
            bool pivotStarted = WasPressed(mouse, GetOrbitPivotButton());
            bool rotateStarted =
                !isTopDown && WasPressed(mouse, GetOrbitRotateButton());
            if (rotateStarted)
            {
                captureRequested = true;
                captureButton = GetOrbitRotateButton();
            }
            else if (panStarted)
            {
                captureRequested = true;
                captureButton = panButton;
            }

            return rotateStarted || panStarted || pivotStarted || scrollStarted;
        }

        private bool AnyConfiguredKeyWasPressed(Keyboard keyboard)
        {
            return WasPressed(keyboard, GetMoveForward()) ||
                   WasPressed(keyboard, GetMoveForwardAlternative()) ||
                   WasPressed(keyboard, GetMoveBackward()) ||
                   WasPressed(keyboard, GetMoveBackwardAlternative()) ||
                   WasPressed(keyboard, GetMoveRight()) ||
                   WasPressed(keyboard, GetMoveRightAlternative()) ||
                   WasPressed(keyboard, GetMoveLeft()) ||
                   WasPressed(keyboard, GetMoveLeftAlternative()) ||
                   WasPressed(keyboard, GetMoveUp()) ||
                   WasPressed(keyboard, GetMoveUpAlternative()) ||
                   WasPressed(keyboard, GetMoveDown()) ||
                   WasPressed(keyboard, GetMoveDownAlternative()) ||
                   WasPressed(keyboard, GetBoost()) ||
                   WasPressed(keyboard, GetBoostAlternative()) ||
                   WasPressed(keyboard, GetSlow()) ||
                   WasPressed(keyboard, GetSlowAlternative());
        }

        private bool IsInputNeutral(
            Mouse mouse,
            Keyboard keyboard,
            DeucarianInputSystemNavigationMode mode)
        {
            return !AnyConfiguredPointerButtonIsPressed(mouse, mode) &&
                   !AnyConfiguredKeyIsPressed(keyboard);
        }

        private bool AnyConfiguredPointerButtonIsPressed(
            Mouse mouse,
            DeucarianInputSystemNavigationMode mode)
        {
            if (mode == DeucarianInputSystemNavigationMode.Fly)
            {
                return IsPressed(mouse, GetFlyLookButton());
            }

            return IsPressed(mouse, GetOrbitRotateButton()) ||
                   IsPressed(mouse, GetOrbitPanButton()) ||
                   IsPressed(mouse, GetOrbitPivotButton());
        }

        private bool AnyConfiguredKeyIsPressed(Keyboard keyboard)
        {
            return IsPressed(keyboard, GetMoveForward()) ||
                   IsPressed(keyboard, GetMoveForwardAlternative()) ||
                   IsPressed(keyboard, GetMoveBackward()) ||
                   IsPressed(keyboard, GetMoveBackwardAlternative()) ||
                   IsPressed(keyboard, GetMoveRight()) ||
                   IsPressed(keyboard, GetMoveRightAlternative()) ||
                   IsPressed(keyboard, GetMoveLeft()) ||
                   IsPressed(keyboard, GetMoveLeftAlternative()) ||
                   IsPressed(keyboard, GetMoveUp()) ||
                   IsPressed(keyboard, GetMoveUpAlternative()) ||
                   IsPressed(keyboard, GetMoveDown()) ||
                   IsPressed(keyboard, GetMoveDownAlternative()) ||
                   IsPressed(keyboard, GetBoost()) ||
                   IsPressed(keyboard, GetBoostAlternative()) ||
                   IsPressed(keyboard, GetSlow()) ||
                   IsPressed(keyboard, GetSlowAlternative());
        }

        private static bool WasPressed(Mouse mouse, DeucarianMouseButton button)
        {
            ButtonControl control =
                DeucarianInputSystemDeviceUtility.GetMouseButton(mouse, button);
            return control != null && control.wasPressedThisFrame;
        }

        private static bool IsPressed(Mouse mouse, DeucarianMouseButton button)
        {
            ButtonControl control =
                DeucarianInputSystemDeviceUtility.GetMouseButton(mouse, button);
            return control != null && control.isPressed;
        }

        private static bool WasPressed(Keyboard keyboard, Key key)
        {
            return keyboard != null &&
                   key != Key.None &&
                   keyboard[key] != null &&
                   keyboard[key].wasPressedThisFrame;
        }

        private static bool IsPressed(Keyboard keyboard, Key key)
        {
            return keyboard != null &&
                   key != Key.None &&
                   keyboard[key] != null &&
                   keyboard[key].isPressed;
        }

        private DeucarianMouseButton GetOrbitRotateButton() =>
            settings != null ? settings.OrbitRotateButton : DeucarianMouseButton.Left;
        private DeucarianMouseButton GetOrbitPanButton() =>
            settings != null ? settings.OrbitPanButton : DeucarianMouseButton.Right;
        private DeucarianMouseButton GetOrbitPivotButton() =>
            settings != null ? settings.OrbitPivotButton : DeucarianMouseButton.Middle;
        private DeucarianMouseButton GetFlyLookButton() =>
            settings != null ? settings.FlyLookButton : DeucarianMouseButton.Right;
        private Key GetMoveForward() => settings != null ? settings.MoveForward : Key.W;
        private Key GetMoveForwardAlternative() =>
            settings != null ? settings.MoveForwardAlternative : Key.UpArrow;
        private Key GetMoveBackward() => settings != null ? settings.MoveBackward : Key.S;
        private Key GetMoveBackwardAlternative() =>
            settings != null ? settings.MoveBackwardAlternative : Key.DownArrow;
        private Key GetMoveRight() => settings != null ? settings.MoveRight : Key.D;
        private Key GetMoveRightAlternative() =>
            settings != null ? settings.MoveRightAlternative : Key.RightArrow;
        private Key GetMoveLeft() => settings != null ? settings.MoveLeft : Key.A;
        private Key GetMoveLeftAlternative() =>
            settings != null ? settings.MoveLeftAlternative : Key.LeftArrow;
        private Key GetMoveUp() => settings != null ? settings.MoveUp : Key.E;
        private Key GetMoveUpAlternative() =>
            settings != null ? settings.MoveUpAlternative : Key.PageUp;
        private Key GetMoveDown() => settings != null ? settings.MoveDown : Key.Q;
        private Key GetMoveDownAlternative() =>
            settings != null ? settings.MoveDownAlternative : Key.PageDown;
        private Key GetBoost() => settings != null ? settings.Boost : Key.LeftShift;
        private Key GetBoostAlternative() =>
            settings != null ? settings.BoostAlternative : Key.RightShift;
        private Key GetSlow() => settings != null ? settings.Slow : Key.LeftCtrl;
        private Key GetSlowAlternative() =>
            settings != null ? settings.SlowAlternative : Key.RightCtrl;
    }
}
