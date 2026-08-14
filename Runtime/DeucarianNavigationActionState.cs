using System;
using UnityEngine;

namespace Deucarian.CameraNavigation.InputSystemIntegration
{
    [Flags]
    public enum DeucarianNavigationActionKinds
    {
        None = 0,
        Pointer = 1,
        Keyboard = 2
    }

    public readonly struct DeucarianNavigationActionState
    {
        public DeucarianNavigationActionState(
            Vector2 pointerPosition,
            DeucarianNavigationActionKinds startedActions,
            bool isNeutral,
            bool escapePressed,
            bool captureRequested,
            DeucarianMouseButton captureButton)
        {
            PointerPosition = pointerPosition;
            StartedActions = startedActions;
            IsNeutral = isNeutral;
            EscapePressed = escapePressed;
            CaptureRequested = captureRequested;
            CaptureButton = captureButton;
        }

        public Vector2 PointerPosition { get; }
        public DeucarianNavigationActionKinds StartedActions { get; }
        public bool IsNeutral { get; }
        public bool EscapePressed { get; }
        public bool CaptureRequested { get; }
        public DeucarianMouseButton CaptureButton { get; }
        public bool HasNewNavigationAction =>
            StartedActions != DeucarianNavigationActionKinds.None;
        public bool HasPointerAction =>
            (StartedActions & DeucarianNavigationActionKinds.Pointer) != 0;
        public bool HasKeyboardAction =>
            (StartedActions & DeucarianNavigationActionKinds.Keyboard) != 0;

        public static DeucarianNavigationActionState None { get; } =
            new DeucarianNavigationActionState(
                Vector2.zero,
                DeucarianNavigationActionKinds.None,
                true,
                false,
                false,
                default);
    }

    public interface IDeucarianNavigationActionStateSource
    {
        DeucarianNavigationActionState ReadActionState(
            DeucarianInputSystemNavigationMode mode,
            bool isTopDown);

        bool IsButtonPressed(DeucarianMouseButton button);

        bool IsOrbitRotatePressed();
    }
}
