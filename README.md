# Deucarian Camera Navigation Input System Integration

## What this is

This package connects Unity's Input System to `com.deucarian.camera-navigation`.
It provides configurable Orbit and Fly input sources plus a ready-to-use rig that
drives the input-agnostic Deucarian controllers.

Package ID: `com.deucarian.camera-navigation.input-system-integration`

Current package version: `0.1.0`.

## When to use it

- You want mouse and keyboard Orbit/Fly navigation to work immediately.
- You want standard controls that remain configurable per project.
- You want UI or application code to block navigation through a narrow interface.

## When not to use it

- You supply touch, XR, gamepad, replay, or network input directly to Camera Navigation.
- You only need poses, framing, or scripted camera movement.
- You need report-viewer commands, model loading, toolbar state, or application logging.

## Install

Install the Camera Navigation core package and this integration through the
Deucarian Package Installer. The integration also installs Unity Input System.

## Quick start

1. Enable Unity's Input System or `Both` under Active Input Handling.
2. Add `Deucarian Input System Camera Navigation Rig` to a GameObject.
3. Assign a camera, or tag the intended camera as `MainCamera`.
4. Optionally assign navigation controls and input settings assets.

Default controls:

- Orbit: left-drag rotate, right-drag pan, middle-click pivot, wheel zoom.
- Fly: right-drag look, wheel dolly.
- Both: WASD/arrows move, Q/E vertical move, Shift boost, Ctrl slow.

## Public API

- `DeucarianOrbitInputSystemSource`: reads normalized Orbit input.
- `DeucarianFlyInputSystemSource`: reads normalized Fly input.
- `DeucarianInputSystemNavigationSettings`: configurable keys, buttons, thresholds, and scroll normalization.
- `IDeucarianNavigationInputBlocker`: application/UI input-blocking boundary.
- `DeucarianInputSystemCameraNavigationRig`: complete Orbit/Fly host with pivot raycasting and model-scale reference bounds.

## Integration boundary

The package maps Unity Input System devices to Camera Navigation input structs.
It does not own application mode commands, report/model lifecycle, UI behavior, or
selection systems. Applications can implement `IDeucarianNavigationInputBlocker`
and can set pivots or reference bounds through the rig.

## Validation

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

## License

See [LICENSE.md](LICENSE.md).
