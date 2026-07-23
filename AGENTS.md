# Deucarian Camera Navigation Input System Integration Agent Notes

Package ID: `com.deucarian.camera-navigation.input-system-integration`
Repository: `Deucarian/CameraNavigation-InputSystem-Integration`

Follow the canonical Deucarian governance docs in Package Registry, especially
capability ownership and dependency rules.

## Ownership

This package owns:

- The optional adapter between Unity Input System devices and Deucarian Camera Navigation.
- The ready-to-use Orbit/Fly input rig composed from those two targets.

Registered capabilities:

- None. This is an integration package.

This package must not own:

- Camera navigation math, application commands, report/model lifecycle, UI frameworks,
  generic input frameworks, selection behavior, logging, or diagnostics.

## Dependencies

Required dependencies:

- `com.deucarian.camera-navigation`: target Orbit/Fly controllers and normalized input.
- `com.unity.inputsystem`: source keyboard, mouse, and pointer device APIs.

Allowed dependency shape:

- Must stay inside the adapter boundary between its declared targets.

## Policies

- Logging: no logging dependency or direct Unity Debug calls.
- Common: do not add Common unless production cleanup directly requires it.
- Editor UI: no editor shell ownership.
- Diagnostics: no diagnostics ownership.
- Testing: test adapter and rig behavior only; tests may use `DestroyImmediate`.

## Validation

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

## Codex Guidance

- Work on `develop`; do not edit or merge `main` unless promotion-only.
- Do not edit `Library/PackageCache`.
- Do not guess dependency versions.
- Keep Package Registry, Package Installer, and Bootstrap catalogs aligned when registered.
- Keep the core Camera Navigation package input-agnostic.
