# Ready-to-use Navigation Rig

1. Set **Active Input Handling** to **Input System Package (New)** or **Both**.
2. Create a GameObject and add `DeucarianInputSystemCameraNavigationRig`.
3. Assign the camera, or tag it `MainCamera`.
4. Set the initial pivot or call `SetReferenceBounds` when content loads.

Alternatively, add `DeucarianReadyRigExample` to a GameObject and assign its
camera and reference renderer. The example adds the complete rig, uses the
renderer bounds as model-scale reference, and initializes the Orbit pivot.

The rig automatically adds its Orbit and Fly input-source components.

Default bindings:

- Left mouse drag: Orbit rotation after a 25-pixel drag threshold.
- Right mouse drag: Orbit pan or Fly look.
- Middle mouse click: choose an Orbit pivot by raycast.
- Mouse wheel: smooth Orbit zoom or Fly dolly.
- WASD or arrow keys: horizontal/forward movement.
- Q/E or Page Down/Page Up: vertical movement.
- Shift: boost.
- Ctrl: precision/slow movement.

Use `SetMode(DeucarianInputSystemNavigationMode.Fly)` to enter Fly mode.
Implement `IDeucarianNavigationInputBlocker` in the consuming application when
navigation must pause over UI or while a text field owns keyboard focus.
