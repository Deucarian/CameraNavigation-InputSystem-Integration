using UnityEngine;

namespace Deucarian.CameraNavigation.InputSystemIntegration.Samples
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DeucarianInputSystemCameraNavigationRig))]
    public sealed class DeucarianReadyRigExample : MonoBehaviour
    {
        [SerializeField] private Camera navigationCamera;
        [SerializeField] private Renderer referenceRenderer;

        private void Awake()
        {
            DeucarianInputSystemCameraNavigationRig rig =
                GetComponent<DeucarianInputSystemCameraNavigationRig>();
            if (navigationCamera != null)
            {
                rig.NavigationCamera = navigationCamera;
            }

            if (referenceRenderer == null)
            {
                return;
            }

            Bounds bounds = referenceRenderer.bounds;
            rig.SetReferenceBounds(bounds);
            rig.SetPivot(bounds.center);
        }
    }
}
