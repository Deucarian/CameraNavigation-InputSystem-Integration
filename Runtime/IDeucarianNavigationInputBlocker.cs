using UnityEngine;

namespace Deucarian.CameraNavigation.InputSystemIntegration
{
    public interface IDeucarianNavigationInputBlocker
    {
        bool IsPointerInputBlocked(Vector2 screenPosition);
        bool IsKeyboardInputBlocked();
    }
}
