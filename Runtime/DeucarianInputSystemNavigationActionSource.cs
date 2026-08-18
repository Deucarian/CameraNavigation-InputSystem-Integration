using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace Deucarian.CameraNavigation.InputSystemIntegration
{
    [DisallowMultipleComponent]
    public sealed class DeucarianInputSystemNavigationActionSource :
        MonoBehaviour,
        IDeucarianNavigationActionStateSource,
        IDeucarianCaptureRequiredActionStateSource
    {
        [SerializeField] private DeucarianInputSystemNavigationSettings settings;

        private bool orbitRotateGesturePending;
        private bool orbitRotateDragActive;
        private Vector2 orbitRotateStartPosition;
        private DeucarianMouseButton trackedOrbitRotateButton;
        private uint orbitRotateCaptureRequestUpdateCount;

        public DeucarianInputSystemNavigationSettings Settings
        {
            get => settings;
            set
            {
                if (settings == value)
                {
                    return;
                }

                settings = value;
                ResetOrbitRotateGesture();
            }
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
            if (IsOrbitRotateCaptureRequest(
                    captureRequested,
                    captureButton))
            {
                // UI blocking must be evaluated where the gesture began. The
                // current pointer may already have crossed out of a UI region.
                pointerPosition = orbitRotateStartPosition;
            }

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

        public bool IsCaptureRequiredPointerActionPressed(
            DeucarianInputSystemNavigationMode mode,
            bool isTopDown)
        {
            if (mode == DeucarianInputSystemNavigationMode.Fly)
            {
                return IsButtonPressed(GetFlyLookButton());
            }

            DeucarianMouseButton rotateButton = GetOrbitRotateButton();
            DeucarianMouseButton panButton = GetOrbitPanButton();
            bool panPressed = IsButtonPressed(panButton) &&
                              (isTopDown || panButton != rotateButton);
            bool rotateDragPressed =
                !isTopDown &&
                orbitRotateDragActive &&
                trackedOrbitRotateButton == rotateButton &&
                IsButtonPressed(rotateButton);
            return panPressed || rotateDragPressed;
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
                ResetOrbitRotateGesture();
                DeucarianMouseButton lookButton = GetFlyLookButton();
                bool lookStarted = WasPressed(mouse, lookButton);
                captureRequested = lookStarted;
                captureButton = lookButton;
                return lookStarted || scrollStarted;
            }

            DeucarianMouseButton rotateButton = GetOrbitRotateButton();
            bool rotatePressedThisFrame =
                !isTopDown && WasPressed(mouse, rotateButton);
            bool rotateDragStarted = UpdateOrbitRotateGesture(
                mouse,
                rotateButton,
                !isTopDown);
            DeucarianMouseButton panButton = GetOrbitPanButton();
            bool rotateOwnsPanPress =
                rotatePressedThisFrame && panButton == rotateButton;
            bool panStarted =
                !rotateOwnsPanPress && WasPressed(mouse, panButton);
            DeucarianMouseButton pivotButton = GetOrbitPivotButton();
            bool rotateOwnsPivotPress =
                rotatePressedThisFrame && pivotButton == rotateButton;
            bool pivotStarted =
                !rotateOwnsPivotPress && WasPressed(mouse, pivotButton);
            if (rotateDragStarted)
            {
                captureRequested = true;
                captureButton = rotateButton;
            }
            else if (panStarted)
            {
                captureRequested = true;
                captureButton = panButton;
            }

            return rotateDragStarted || panStarted || pivotStarted || scrollStarted;
        }

        private bool UpdateOrbitRotateGesture(
            Mouse mouse,
            DeucarianMouseButton rotateButton,
            bool enabled)
        {
            if (!enabled || mouse == null)
            {
                ResetOrbitRotateGesture();
                return false;
            }

            if (orbitRotateGesturePending &&
                trackedOrbitRotateButton != rotateButton)
            {
                ResetOrbitRotateGesture();
            }

            ButtonControl rotateControl =
                DeucarianInputSystemDeviceUtility.GetMouseButton(
                    mouse,
                    rotateButton);
            if (rotateControl == null || !rotateControl.isPressed)
            {
                ResetOrbitRotateGesture();
                return false;
            }

            if (rotateControl.wasPressedThisFrame)
            {
                orbitRotateGesturePending = true;
                orbitRotateDragActive = false;
                orbitRotateStartPosition = mouse.position.ReadValue();
                trackedOrbitRotateButton = rotateButton;
                orbitRotateCaptureRequestUpdateCount = default;
            }

            if (!orbitRotateGesturePending)
            {
                return false;
            }

            if (orbitRotateDragActive)
            {
                return orbitRotateCaptureRequestUpdateCount ==
                       InputState.updateCount;
            }

            float threshold = settings != null
                ? settings.OrbitDragThreshold
                : DeucarianInputSystemNavigationSettings
                    .DefaultOrbitDragThreshold;
            if (Vector2.Distance(
                    orbitRotateStartPosition,
                    mouse.position.ReadValue()) < threshold)
            {
                return false;
            }

            orbitRotateDragActive = true;
            orbitRotateCaptureRequestUpdateCount = InputState.updateCount;
            return true;
        }

        private bool IsOrbitRotateCaptureRequest(
            bool captureRequested,
            DeucarianMouseButton captureButton)
        {
            return captureRequested &&
                   orbitRotateDragActive &&
                   orbitRotateCaptureRequestUpdateCount ==
                       InputState.updateCount &&
                   captureButton == trackedOrbitRotateButton;
        }

        private void OnDisable()
        {
            ResetOrbitRotateGesture();
        }

        private void ResetOrbitRotateGesture()
        {
            orbitRotateGesturePending = false;
            orbitRotateDragActive = false;
            orbitRotateStartPosition = default;
            trackedOrbitRotateButton = default;
            orbitRotateCaptureRequestUpdateCount = default;
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
