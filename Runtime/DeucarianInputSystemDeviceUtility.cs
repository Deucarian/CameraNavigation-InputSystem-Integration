using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Deucarian.CameraNavigation.InputSystemIntegration
{
    internal static class DeucarianInputSystemDeviceUtility
    {
        internal static ButtonControl GetMouseButton(
            Mouse mouse,
            DeucarianMouseButton button)
        {
            if (mouse == null)
            {
                return null;
            }

            switch (button)
            {
                case DeucarianMouseButton.Right:
                    return mouse.rightButton;
                case DeucarianMouseButton.Middle:
                    return mouse.middleButton;
                case DeucarianMouseButton.Forward:
                    return mouse.forwardButton;
                case DeucarianMouseButton.Back:
                    return mouse.backButton;
                default:
                    return mouse.leftButton;
            }
        }

        internal static Vector3 ReadMovement(
            Keyboard keyboard,
            DeucarianInputSystemNavigationSettings settings)
        {
            if (keyboard == null)
            {
                return Vector3.zero;
            }

            float x =
                ReadAxis(
                    keyboard,
                    GetMoveRight(settings),
                    GetMoveRightAlternative(settings),
                    GetMoveLeft(settings),
                    GetMoveLeftAlternative(settings));
            float y =
                ReadAxis(
                    keyboard,
                    GetMoveUp(settings),
                    GetMoveUpAlternative(settings),
                    GetMoveDown(settings),
                    GetMoveDownAlternative(settings));
            float z =
                ReadAxis(
                    keyboard,
                    GetMoveForward(settings),
                    GetMoveForwardAlternative(settings),
                    GetMoveBackward(settings),
                    GetMoveBackwardAlternative(settings));
            Vector3 movement = new Vector3(x, y, z);
            return movement.sqrMagnitude > 1f ? movement.normalized : movement;
        }

        internal static bool ReadBoost(
            Keyboard keyboard,
            DeucarianInputSystemNavigationSettings settings)
        {
            return IsPressed(keyboard, GetBoost(settings)) ||
                   IsPressed(keyboard, GetBoostAlternative(settings));
        }

        internal static bool ReadSlow(
            Keyboard keyboard,
            DeucarianInputSystemNavigationSettings settings)
        {
            return IsPressed(keyboard, GetSlow(settings)) ||
                   IsPressed(keyboard, GetSlowAlternative(settings));
        }

        internal static float NormalizeScroll(
            float scroll,
            DeucarianInputSystemNavigationSettings settings)
        {
            float normalization = settings != null
                ? settings.ScrollNormalization
                : DeucarianInputSystemNavigationSettings.DefaultScrollNormalization;
            return Mathf.Abs(scroll) <= 0.0001f
                ? 0f
                : scroll / Mathf.Max(0.0001f, normalization);
        }

        private static float ReadAxis(
            Keyboard keyboard,
            Key positive,
            Key positiveAlternative,
            Key negative,
            Key negativeAlternative)
        {
            bool isPositive =
                IsPressed(keyboard, positive) ||
                IsPressed(keyboard, positiveAlternative);
            bool isNegative =
                IsPressed(keyboard, negative) ||
                IsPressed(keyboard, negativeAlternative);
            return (isPositive ? 1f : 0f) - (isNegative ? 1f : 0f);
        }

        private static bool IsPressed(Keyboard keyboard, Key key)
        {
            return keyboard != null &&
                   key != Key.None &&
                   keyboard[key] != null &&
                   keyboard[key].isPressed;
        }

        private static Key GetMoveForward(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveForward : Key.W;
        private static Key GetMoveForwardAlternative(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveForwardAlternative : Key.UpArrow;
        private static Key GetMoveBackward(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveBackward : Key.S;
        private static Key GetMoveBackwardAlternative(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveBackwardAlternative : Key.DownArrow;
        private static Key GetMoveRight(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveRight : Key.D;
        private static Key GetMoveRightAlternative(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveRightAlternative : Key.RightArrow;
        private static Key GetMoveLeft(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveLeft : Key.A;
        private static Key GetMoveLeftAlternative(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveLeftAlternative : Key.LeftArrow;
        private static Key GetMoveUp(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveUp : Key.E;
        private static Key GetMoveUpAlternative(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveUpAlternative : Key.PageUp;
        private static Key GetMoveDown(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveDown : Key.Q;
        private static Key GetMoveDownAlternative(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.MoveDownAlternative : Key.PageDown;
        private static Key GetBoost(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.Boost : Key.LeftShift;
        private static Key GetBoostAlternative(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.BoostAlternative : Key.RightShift;
        private static Key GetSlow(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.Slow : Key.LeftCtrl;
        private static Key GetSlowAlternative(DeucarianInputSystemNavigationSettings value) =>
            value != null ? value.SlowAlternative : Key.RightCtrl;
    }
}
