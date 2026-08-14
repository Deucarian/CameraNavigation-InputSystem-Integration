# Changelog

## 0.1.7 - 2026-08-14

- Added an additive configured-action capability that lets viewer-level
  pointer-capture coordination distinguish drag actions from scrolling and
  other pointer actions without polling raw Input System devices.

## 0.1.6 - 2026-08-14

- Added a configurable navigation-action state source so viewer-level
  coordination and pointer capture can observe the same remapped Input System
  controls as the Orbit/Fly adapters without polling raw devices themselves.

## 0.1.5 - 2026-07-23

- Aligned the exact Camera Navigation dependency with the approved tuned
  default Orbit and Fly profile.

## 0.1.4 - 2026-07-23

- Aligned the Camera Navigation dependency with the canonical adjustable
  controls contract.

## 0.1.3 - 2026-07-23

- Added configurable pointer-delta scaling and restored the `0.1` default used
  by the Report Viewer's pre-extraction legacy mouse axes.
- Aligned the Camera Navigation dependency with the restored legacy tuning
  defaults.

## 0.1.2 - 2026-07-23

- Aligned the exact Camera Navigation dependency with its shared Deucarian
  Editor-styled settings workflow release.

## 0.1.1 - 2026-07-23

- Aligned the exact Camera Navigation dependency with the configurable legacy
  speed-profile release.

## 0.1.0 - 2026-07-23

- Added configurable Unity Input System Orbit and Fly sources.
- Added a ready-to-use navigation rig with pivot raycasting and reference bounds.
- Added application/UI input-blocking integration points.
- Added a compiled ready-rig sample.
- Added EditMode coverage for device mapping, rig behavior, and projection modes.
