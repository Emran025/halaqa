# Windows release

The desktop application is published by `.github/workflows/windows-release.yml` as a self-contained `win-x64` single-file executable. The published ZIP includes the required application assets and does not require the .NET runtime to be installed on the target Windows device.

Before creating a release, add the repository secret `HALAQA_API_BASE_URL` with the production API base URL, including the `/api/v1/` path when required. The workflow validates that the value is an absolute HTTPS URL, injects it only into the generated release `appsettings.json`, and never commits it to the repository or prints it in logs.

The workflow runs the full solution build and tests before publishing. It creates a ZIP package and a SHA-256 checksum, uploads both to the workflow run, and attaches both files to the GitHub Release. Pushing a tag such as `v0.0.1` starts the release automatically; the workflow can also be started manually with a release tag input.

The checked-in `appsettings.json` intentionally contains an empty `Api:BaseUrl`. This prevents a development or local address from being shipped accidentally. A local developer can provide the value at runtime with the environment variable `HALAQA_API__BASEURL`.
