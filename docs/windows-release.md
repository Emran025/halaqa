# Windows release

The desktop application is published by `.github/workflows/windows-release.yml` as self-contained single-file Windows applications. Every architecture package contains `Halaqa.Desktop.exe`, the required application assets, and the runtime needed by that architecture; the target device does not need .NET installed.

The workflow produces three packages:

- `win-x64` for standard 64-bit Intel/AMD Windows devices.
- `win-x86` for 32-bit Intel/AMD Windows devices.
- `win-arm64` for native Windows on ARM64 devices. Windows on ARM can generally run the x64 package through emulation when native ARM64 is not required.

Before creating a release, add the repository secret `HALAQA_API_BASE_URL` with the production API base URL, including the `/api/v1/` path when required. The workflow validates that the value is an absolute HTTPS URL, injects it only into each generated release `appsettings.json`, and never commits it to the repository or prints it in logs.

The workflow first runs the full solution build and tests. It then publishes each runtime independently, creates a ZIP package and SHA-256 checksum for each architecture, verifies every checksum, and attaches all packages to the GitHub Release. Pushing a tag such as `v0.0.2` starts the release automatically; the workflow can also be started manually with a release tag input.

The checked-in `appsettings.json` intentionally contains an empty `Api:BaseUrl`. This prevents a development or local address from being shipped accidentally. A local developer can provide the value at runtime with the environment variable `HALAQA_API__BASEURL`.
