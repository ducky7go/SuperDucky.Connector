# SuperDucky.Connector

## Project overview

A mod for the Unity game "Escape from Duckov", built on **Ducky.Sdk** (0.1.8, `Directory.Packages.props`). It exports an item encyclopedia (JSON metadata, multi-language descriptions, PNG icons) and records item acquisition history to files under the system temp directory (`Path.GetTempPath()/SuperDucky.Connector/`), so external tools can read them. It ships as a NuGet package (`info.ini` metadata, packed by `ducky7go/action-ducky-nuget`). Mod name `SuperDucky.Connector`, license MIT.

## Repository layout

- `SuperDucky.Connector/` the only project (`SuperDucky.Connector.csproj`, references `Ducky.Sdk`)
  - `ModBehaviour.cs` entry point (`ModBehaviourBase`: `ModEnabled` / `ModDisabled`, hooks `SceneLoader.onAfterSceneInitialize`)
  - `ItemDictionaryTracker.cs` (item encyclopedia export), `ItemAcquisitionMonitor.cs` (inventory monitoring), `FileStorageService.cs` (file I/O; digit bucket = `Math.Abs(itemId) % 10`), `Models/`
  - `assets/` `info.ini`, `preview.png`, `description/en.md`, `description/zh.md`
  - `Docs/data-structure.md`, `Docs/api-examples.md`: the documented on-disk format for external consumers
- `scripts/` `fetch_build_dependency.sh` (downloads `Managed/` game assemblies from the `ducky7go/sdk_build_dependency` release), `setup_build_tools.sh` (dotnet-script 1.5.0, dotnet-ilrepack 2.0.44)
- Root: `SuperDucky.Connector.slnx`, `Directory.Build.props`, `Directory.Packages.props` (central package management), `global.json` (SDK 9.0.301), `GitVersion.yml`, `nuget.config` (MyGet feed mapped only to `Ducky.Sdk`, nuget.org for the rest)

## Build / test / run

CI builds like this (see `.github/workflows/publish.yml` and `pr-build-validation.yml`):

1. `bash scripts/fetch_build_dependency.sh` (downloads game assemblies into `./Managed`)
2. install global tools `dotnet-script` 1.5.0 and `dotnet-ilrepack` 2.0.44 (`scripts/setup_build_tools.sh` does the same locally)
3. `CI=true dotnet build SuperDucky.Connector/SuperDucky.Connector.csproj -p:PackageVersion=<ver> -p:Configuration=Release`. Output goes to `artifacts/Mods/SuperDucky.Connector/`. Ducky.Sdk uses the git root when `CI` is set (see `Directory.Build.props`).

Locally, `Directory.Build.props` imports an optional `Directory.Build.local.props`, and the csproj imports an optional `../Local.props`. Both are git-ignored and hold machine-specific game paths. There is no test project.

## CI / release

- PRs to main: `pr-build-validation.yml` builds, packs with `push: false`, uploads an artifact and comments on the PR.
- `publish.yml` runs on tags `*.*.*` and on pushes to main. The version is the tag, or GitVersion `fullSemVer` for dev builds (`GitVersion.yml`, main label `dev`). It rewrites `version =` in the output `info.ini`, logs into NuGet via OIDC (`NUGET_USER` secret), and pushes only for tags or main.
- `release-drafter.yml` plus `.github/release-drafter.yml` group release notes by PR label (Breaking Change, New Feature, Feature, Bug Fix, Improvement, Docs).

## Conventions

- Target `netstandard2.1`, `LangVersion preview`, nullable enabled, implicit usings.
- Log prefix in code: `[DuckyConnector]`. Unity API calls on textures must stay on the main thread.
- Changing the on-disk format means updating `Docs/data-structure.md` and `Docs/api-examples.md`, since external tools depend on them.
- Mod descriptions are bilingual (`assets/description/en.md`, `zh.md`).
- Commit style: `docs(connector): ...` (conventional commits, with PR number).
- Behavior or spec changes follow the OpenSpec workflow.

## Gotchas

- `info.ini` has `version = 0.1.0`. CI overwrites it at build time, so do not bump it by hand for releases.
- The build needs network access (MyGet, nuget.org, GitHub) and the `Managed/` assemblies. A plain `dotnet build` on a fresh clone will not work.

## Related repositories (ducky7go)

- `DML`: the SuperDucky desktop app. Its save gallery and launcher-dependency features use this mod.
- `SuperDucky.docs`: user docs for the app. The `enable-save-gallery` guide tells users to install `SuperDucky.Connector` from the NuGet market, so keep it in sync with this mod's name and package.
- `Ducky.Sdk` and `sdk_build_dependency`: the SDK and game assemblies this mod builds against. `action-ducky-nuget` packs it.
