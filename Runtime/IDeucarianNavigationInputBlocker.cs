using UnityEngine;

namespace Deucarian.CameraNavigation.InputSystemIntegration
{
    public interface IDeucarianNavigationInputBlocker
    {
        bool IsPointerInputBlocked(Vector2 screenPosition);
        bool IsKeyboardInputBlocked();
    }

    public interface IDeucarianNavigationGestureStartBlocker
    {
        // Pending capture can block this frame without rejecting the gesture.
        bool IsPointerGestureStartBlocked(Vector2 screenPosition);
    }
}
