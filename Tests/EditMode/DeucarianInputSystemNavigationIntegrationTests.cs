using Deucarian.CameraNavigation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace Deucarian.CameraNavigation.InputSystemIntegration.Tests
{
    public sealed class DeucarianInputSystemNavigationIntegrationTests : InputTestFixture
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void InputSettingsExposeReadyToUseDefaultsAndConfiguration()
        {
            DeucarianInputSystemNavigationSettings settings =
                ScriptableObject.CreateInstance<DeucarianInputSystemNavigationSettings>();
            try
            {
                Assert.That(settings.OrbitRotateButton, Is.EqualTo(DeucarianMouseButton.Left));
                Assert.That(settings.OrbitPanButton, Is.EqualTo(DeucarianMouseButton.Right));
                Assert.That(settings.OrbitPivotButton, Is.EqualTo(DeucarianMouseButton.Middle));
                Assert.That(settings.FlyLookButton, Is.EqualTo(DeucarianMouseButton.Right));
                Assert.That(settings.MoveForward, Is.EqualTo(Key.W));
                Assert.That(settings.MoveUp, Is.EqualTo(Key.E));
                Assert.That(
                    settings.OrbitDragThreshold,
                    Is.EqualTo(
                        DeucarianInputSystemNavigationSettings.DefaultOrbitDragThreshold));
                Assert.That(
                    settings.ScrollNormalization,
                    Is.EqualTo(
                        DeucarianInputSystemNavigationSettings.DefaultScrollNormalization));
                Assert.That(
                    settings.PointerDeltaScale,
                    Is.EqualTo(
                        DeucarianInputSystemNavigationSettings.DefaultPointerDeltaScale));

                settings.OrbitRotateButton = DeucarianMouseButton.Middle;
                settings.OrbitDragThreshold = 12f;
                settings.PointerDeltaScale = 0.25f;
                settings.ScrollNormalization = 1f;

                Assert.That(
                    settings.OrbitRotateButton,
                    Is.EqualTo(DeucarianMouseButton.Middle));
                Assert.That(settings.OrbitDragThreshold, Is.EqualTo(12f));
                Assert.That(settings.PointerDeltaScale, Is.EqualTo(0.25f));
                Assert.That(settings.ScrollNormalization, Is.EqualTo(1f));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void RigAddsReadyToUseOrbitAndFlySources()
        {
            GameObject rigObject = new GameObject("Navigation Rig");
            try
            {
                rigObject.AddComponent<DeucarianInputSystemCameraNavigationRig>();

                Assert.That(
                    rigObject.GetComponent<DeucarianOrbitInputSystemSource>(),
                    Is.Not.Null);
                Assert.That(
                    rigObject.GetComponent<DeucarianFlyInputSystemSource>(),
                    Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(rigObject);
            }
        }

        [Test]
        public void OrbitInputSourceMapsDevicesAndHonorsApplicationBlocking()
        {
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GameObject sourceObject = new GameObject("Orbit Input Source");
            GameObject blockerObject = new GameObject("Navigation Input Blocker");
            try
            {
                DeucarianOrbitInputSystemSource source =
                    sourceObject.AddComponent<DeucarianOrbitInputSystemSource>();
                TestNavigationInputBlocker blocker =
                    blockerObject.AddComponent<TestNavigationInputBlocker>();
                InputSystem.QueueStateEvent(
                    keyboard,
                    new KeyboardState(Key.W, Key.E, Key.LeftShift));
                InputSystem.QueueStateEvent(
                    mouse,
                    new MouseState
                    {
                        position = new Vector2(200f, 100f),
                        delta = new Vector2(6f, -2f),
                        scroll = new Vector2(0f, 120f)
                    }.WithButton(MouseButton.Right));
                InputSystem.Update();

                DeucarianOrbitInputSystemFrame frame = source.ReadFrame();

                Assert.That(
                    frame.NavigationInput.Pan,
                    Is.EqualTo(new Vector2(0.6f, -0.2f)));
                Assert.That(frame.NavigationInput.Zoom, Is.EqualTo(1f));
                Assert.That(frame.NavigationInput.Move.y, Is.GreaterThan(0f));
                Assert.That(frame.NavigationInput.Move.z, Is.GreaterThan(0f));
                Assert.IsTrue(frame.NavigationInput.Boost);
                Assert.IsFalse(frame.PivotRequested);

                blocker.BlockPointer = true;
                blocker.BlockKeyboard = true;
                source.SetInputBlocker(blocker);

                DeucarianOrbitInputSystemFrame blockedFrame = source.ReadFrame();

                Assert.That(blockedFrame.NavigationInput, Is.EqualTo(DeucarianOrbitCameraInput.None));
                Assert.IsFalse(blockedFrame.PivotRequested);
            }
            finally
            {
                Object.DestroyImmediate(sourceObject);
                Object.DestroyImmediate(blockerObject);
            }
        }

        [Test]
        public void FlyInputSourceAppliesConfiguredPointerDeltaScale()
        {
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GameObject sourceObject = new GameObject("Fly Input Source");
            DeucarianInputSystemNavigationSettings settings =
                ScriptableObject.CreateInstance<DeucarianInputSystemNavigationSettings>();
            try
            {
                settings.PointerDeltaScale = 0.25f;
                DeucarianFlyInputSystemSource source =
                    sourceObject.AddComponent<DeucarianFlyInputSystemSource>();
                source.Settings = settings;
                InputSystem.QueueStateEvent(
                    mouse,
                    new MouseState
                    {
                        position = new Vector2(200f, 100f),
                        delta = new Vector2(8f, -4f)
                    }.WithButton(MouseButton.Right));
                InputSystem.Update();

                DeucarianFlyCameraInput input = source.ReadInput();

                Assert.That(input.Look, Is.EqualTo(new Vector2(2f, -1f)));
            }
            finally
            {
                Object.DestroyImmediate(sourceObject);
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void InjectedOrbitZoomStopsBeforePivotAndRemainsStable()
        {
            GameObject rigObject = new GameObject("Orbit Rig");
            GameObject cameraObject = new GameObject("Orbit Camera");
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.nearClipPlane = 0.001f;
                camera.transform.position = new Vector3(0f, 0f, -5f);
                camera.transform.rotation =
                    Quaternion.LookRotation(Vector3.forward, Vector3.up);
                DeucarianInputSystemCameraNavigationRig rig =
                    rigObject.AddComponent<DeucarianInputSystemCameraNavigationRig>();
                rig.NavigationCamera = camera;
                rig.Controls = controls;
                rig.SetPivot(Vector3.zero);
                rig.SetReferenceBounds(new Bounds(Vector3.zero, Vector3.one * 0.01f));

                Vector3 startingDirection =
                    (camera.transform.position - rig.OrbitPivot).normalized;
                DeucarianOrbitInputSystemFrame zoomFrame =
                    new DeucarianOrbitInputSystemFrame(
                        new DeucarianOrbitCameraInput(
                            Vector3.zero,
                            Vector2.zero,
                            Vector2.zero,
                            100f,
                            false,
                            false),
                        false,
                        Vector2.zero);
                rig.ApplyOrbitFrame(zoomFrame, 1f / 60f);
                for (int i = 0; i < 240; i++)
                {
                    rig.ApplyOrbitFrame(
                        DeucarianOrbitInputSystemFrame.None,
                        1f / 60f);
                }

                float minimumDistance = rig.GetMinimumOrbitDistance();
                float finalDistance = rig.GetOrbitDistance();
                Vector3 finalDirection =
                    (camera.transform.position - rig.OrbitPivot).normalized;
                Assert.That(finalDistance, Is.EqualTo(minimumDistance).Within(Tolerance));
                Assert.That(finalDistance, Is.LessThan(0.01f));
                Assert.That(
                    Vector3.Dot(startingDirection, finalDirection),
                    Is.GreaterThan(0.9999f));
                Assert.That(rig.OrbitPivot, Is.EqualTo(Vector3.zero));
                AssertFinite(camera.transform.position);
                AssertFinite(camera.transform.rotation);
            }
            finally
            {
                Object.DestroyImmediate(rigObject);
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(controls);
            }
        }

        [Test]
        public void InjectedOrbitRotationPreservesPivotDistance()
        {
            GameObject rigObject = new GameObject("Orbit Rotation Rig");
            GameObject cameraObject = new GameObject("Orbit Rotation Camera");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.transform.rotation =
                    Quaternion.LookRotation(Vector3.forward, Vector3.up);
                DeucarianInputSystemCameraNavigationRig rig =
                    rigObject.AddComponent<DeucarianInputSystemCameraNavigationRig>();
                rig.NavigationCamera = camera;
                rig.SetPivot(new Vector3(1f, 2f, 3f));
                float startingDistance = rig.GetOrbitDistance();

                for (int i = 0; i < 100; i++)
                {
                    rig.ApplyOrbitFrame(
                        new DeucarianOrbitInputSystemFrame(
                            new DeucarianOrbitCameraInput(
                                Vector3.zero,
                                new Vector2(8f, -4f),
                                Vector2.zero,
                                0f,
                                false,
                                false),
                            false,
                            Vector2.zero),
                        1f / 60f);
                }

                Vector3 directionToPivot =
                    (rig.OrbitPivot - camera.transform.position).normalized;
                Assert.That(
                    rig.GetOrbitDistance(),
                    Is.EqualTo(startingDistance).Within(Tolerance));
                Assert.That(
                    Vector3.Dot(camera.transform.forward, directionToPivot),
                    Is.GreaterThan(0.999f));
                Assert.That(rig.OrbitPivot, Is.EqualTo(new Vector3(1f, 2f, 3f)));
                AssertFinite(camera.transform.position);
                AssertFinite(camera.transform.rotation);
            }
            finally
            {
                Object.DestroyImmediate(rigObject);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void RigKeepsOrthographicOrbitAndPerspectiveFlyFunctional()
        {
            GameObject rigObject = new GameObject("Projection Rig");
            GameObject cameraObject = new GameObject("Projection Camera");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.transform.rotation =
                    Quaternion.LookRotation(Vector3.forward, Vector3.up);
                DeucarianInputSystemCameraNavigationRig rig =
                    rigObject.AddComponent<DeucarianInputSystemCameraNavigationRig>();
                rig.NavigationCamera = camera;
                rig.SetPivot(Vector3.zero);

                rig.ApplyOrbitFrame(
                    new DeucarianOrbitInputSystemFrame(
                        new DeucarianOrbitCameraInput(
                            Vector3.zero,
                            Vector2.zero,
                            Vector2.zero,
                            1f,
                            false,
                            false),
                        false,
                        Vector2.zero),
                    1f / 60f);
                for (int i = 0; i < 120; i++)
                {
                    rig.ApplyOrbitFrame(
                        DeucarianOrbitInputSystemFrame.None,
                        1f / 60f);
                }

                Assert.That(camera.orthographicSize, Is.LessThan(5f));
                Assert.IsTrue(camera.orthographic);

                camera.orthographic = false;
                rig.SetMode(DeucarianInputSystemNavigationMode.Fly);
                Vector3 positionBeforeFly = camera.transform.position;
                rig.ApplyFlyInput(
                    new DeucarianFlyCameraInput(
                        Vector2.zero,
                        Vector3.forward,
                        0f,
                        false,
                        false),
                    1f);

                Assert.That(
                    Vector3.Distance(positionBeforeFly, camera.transform.position),
                    Is.GreaterThan(1f));
                Assert.IsFalse(camera.orthographic);
                AssertFinite(camera.transform.position);
            }
            finally
            {
                Object.DestroyImmediate(rigObject);
                Object.DestroyImmediate(cameraObject);
            }
        }

        private static void AssertFinite(Vector3 value)
        {
            Assert.That(float.IsNaN(value.x) || float.IsInfinity(value.x), Is.False);
            Assert.That(float.IsNaN(value.y) || float.IsInfinity(value.y), Is.False);
            Assert.That(float.IsNaN(value.z) || float.IsInfinity(value.z), Is.False);
        }

        private static void AssertFinite(Quaternion value)
        {
            Assert.That(float.IsNaN(value.x) || float.IsInfinity(value.x), Is.False);
            Assert.That(float.IsNaN(value.y) || float.IsInfinity(value.y), Is.False);
            Assert.That(float.IsNaN(value.z) || float.IsInfinity(value.z), Is.False);
            Assert.That(float.IsNaN(value.w) || float.IsInfinity(value.w), Is.False);
        }
    }

    internal sealed class TestNavigationInputBlocker :
        MonoBehaviour,
        IDeucarianNavigationInputBlocker
    {
        internal bool BlockPointer { get; set; }
        internal bool BlockKeyboard { get; set; }

        public bool IsPointerInputBlocked(Vector2 screenPosition)
        {
            return BlockPointer;
        }

        public bool IsKeyboardInputBlocked()
        {
            return BlockKeyboard;
        }
    }
}
