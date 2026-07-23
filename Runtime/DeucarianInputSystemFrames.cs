using Deucarian.CameraNavigation;
using UnityEngine;

namespace Deucarian.CameraNavigation.InputSystemIntegration
{
    public readonly struct DeucarianOrbitInputSystemFrame
    {
        public DeucarianOrbitInputSystemFrame(
            DeucarianOrbitCameraInput navigationInput,
            bool pivotRequested,
            Vector2 pivotScreenPosition)
        {
            NavigationInput = navigationInput;
            PivotRequested = pivotRequested;
            PivotScreenPosition = pivotScreenPosition;
        }

        public DeucarianOrbitCameraInput NavigationInput { get; }
        public bool PivotRequested { get; }
        public Vector2 PivotScreenPosition { get; }

        public static DeucarianOrbitInputSystemFrame None { get; } =
            new DeucarianOrbitInputSystemFrame(
                DeucarianOrbitCameraInput.None,
                false,
                Vector2.zero);
    }
}
