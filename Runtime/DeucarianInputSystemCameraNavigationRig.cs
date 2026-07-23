using Deucarian.CameraNavigation;
using UnityEngine;

namespace Deucarian.CameraNavigation.InputSystemIntegration
{
    public enum DeucarianInputSystemNavigationMode
    {
        Orbit,
        Fly
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(DeucarianOrbitInputSystemSource))]
    [RequireComponent(typeof(DeucarianFlyInputSystemSource))]
    public sealed class DeucarianInputSystemCameraNavigationRig : MonoBehaviour
    {
        [SerializeField] private Camera navigationCamera;
        [SerializeField] private DeucarianCameraNavigationControls controls;
        [SerializeField] private DeucarianInputSystemNavigationSettings inputSettings;
        [SerializeField] private DeucarianInputSystemNavigationMode mode;
        [SerializeField] private bool allowOrbitRotation = true;
        [SerializeField] private LayerMask pivotLayers = ~0;
        [SerializeField] private QueryTriggerInteraction pivotTriggerInteraction =
            QueryTriggerInteraction.Ignore;
        [Tooltip("Optional MonoBehaviour implementing IDeucarianNavigationInputBlocker.")]
        [SerializeField] private MonoBehaviour inputBlocker;

        private readonly DeucarianOrbitCameraController orbitController =
            new DeucarianOrbitCameraController();
        private readonly DeucarianFlyCameraController flyController =
            new DeucarianFlyCameraController();
        private DeucarianOrbitInputSystemSource orbitInput;
        private DeucarianFlyInputSystemSource flyInput;
        private DeucarianInputSystemNavigationMode synchronizedMode;
        private bool hasSynchronizedMode;

        public Camera NavigationCamera
        {
            get => ResolveCamera();
            set
            {
                navigationCamera = value;
                SyncNavigationState();
            }
        }

        public DeucarianCameraNavigationControls Controls
        {
            get => controls;
            set
            {
                controls = value;
                SyncNavigationState();
            }
        }

        public DeucarianInputSystemNavigationSettings InputSettings
        {
            get => inputSettings;
            set
            {
                inputSettings = value;
                ApplyInputConfiguration();
            }
        }

        public DeucarianInputSystemNavigationMode Mode => mode;
        public Vector3 OrbitPivot => orbitController.Pivot;

        private void Awake()
        {
            ResolveInputSources();
            ApplyInputConfiguration();
            SyncNavigationState();
        }

        private void OnEnable()
        {
            ResolveInputSources();
            ApplyInputConfiguration();
            SyncNavigationState();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            Camera camera = ResolveCamera();
            if (camera == null)
            {
                return;
            }

            ResolveInputSources();
            SynchronizeModeIfNeeded();
            if (mode == DeucarianInputSystemNavigationMode.Fly)
            {
                ApplyFlyInput(
                    flyInput != null
                        ? flyInput.ReadInput()
                        : DeucarianFlyCameraInput.None,
                    deltaTime);
                return;
            }

            ApplyOrbitFrame(
                orbitInput != null
                    ? orbitInput.ReadFrame()
                    : DeucarianOrbitInputSystemFrame.None,
                deltaTime);
        }

        public void SetMode(DeucarianInputSystemNavigationMode navigationMode)
        {
            if (mode == navigationMode && hasSynchronizedMode)
            {
                return;
            }

            mode = navigationMode;
            SyncNavigationState();
        }

        public void SetPivot(Vector3 pivot)
        {
            orbitController.SetPivot(pivot);
            orbitController.SyncZoomState(ResolveCamera(), controls);
        }

        public void SetReferenceBounds(Bounds bounds)
        {
            orbitController.SetReferenceBounds(bounds);
            orbitController.SyncZoomState(ResolveCamera(), controls);
        }

        public void SetReferenceScale(float scale)
        {
            orbitController.SetReferenceScale(scale);
            orbitController.SyncZoomState(ResolveCamera(), controls);
        }

        public void ClearReferenceBounds()
        {
            orbitController.ClearReferenceBounds();
            orbitController.SyncZoomState(ResolveCamera(), controls);
        }

        public void SetInputBlocker(MonoBehaviour blocker)
        {
            inputBlocker = blocker;
            ApplyInputConfiguration();
        }

        public void ApplyOrbitFrame(
            DeucarianOrbitInputSystemFrame frame,
            float deltaTime)
        {
            Camera camera = ResolveCamera();
            if (camera == null)
            {
                return;
            }

            if (frame.PivotRequested)
            {
                SelectPivot(camera, frame.PivotScreenPosition);
            }

            orbitController.Apply(
                camera,
                frame.NavigationInput,
                deltaTime,
                controls,
                allowOrbitRotation);
        }

        public void ApplyFlyInput(
            DeucarianFlyCameraInput input,
            float deltaTime)
        {
            Camera camera = ResolveCamera();
            if (camera == null)
            {
                return;
            }

            flyController.Apply(
                camera,
                input,
                deltaTime,
                controls,
                orbitController.GetDistanceToPivot(camera, controls));
        }

        public float GetMinimumOrbitDistance()
        {
            return orbitController.GetMinimumDistance(ResolveCamera(), controls);
        }

        public float GetOrbitDistance()
        {
            return orbitController.GetDistanceToPivot(ResolveCamera(), controls);
        }

        public void SyncNavigationState()
        {
            orbitController.SyncZoomState(ResolveCamera(), controls);
            flyController.SyncZoomState();
            synchronizedMode = mode;
            hasSynchronizedMode = true;
        }

        private void SelectPivot(Camera camera, Vector2 screenPosition)
        {
            Ray ray = camera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    Mathf.Infinity,
                    pivotLayers,
                    pivotTriggerInteraction))
            {
                SetPivot(hit.point);
                return;
            }

            SetPivot(ray.GetPoint(orbitController.GetDistanceToPivot(camera, controls)));
        }

        private void ResolveInputSources()
        {
            if (orbitInput == null)
            {
                orbitInput = GetComponent<DeucarianOrbitInputSystemSource>();
                if (orbitInput == null)
                {
                    orbitInput = gameObject.AddComponent<DeucarianOrbitInputSystemSource>();
                }
            }

            if (flyInput == null)
            {
                flyInput = GetComponent<DeucarianFlyInputSystemSource>();
                if (flyInput == null)
                {
                    flyInput = gameObject.AddComponent<DeucarianFlyInputSystemSource>();
                }
            }
        }

        private void ApplyInputConfiguration()
        {
            ResolveInputSources();
            orbitInput.Settings = inputSettings;
            flyInput.Settings = inputSettings;
            orbitInput.SetInputBlocker(inputBlocker);
            flyInput.SetInputBlocker(inputBlocker);
        }

        private Camera ResolveCamera()
        {
            if (navigationCamera == null)
            {
                navigationCamera = Camera.main;
            }

            return navigationCamera;
        }

        private void SynchronizeModeIfNeeded()
        {
            if (!hasSynchronizedMode || synchronizedMode != mode)
            {
                SyncNavigationState();
            }
        }
    }
}
