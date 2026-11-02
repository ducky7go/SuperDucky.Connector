## OpenSpec

This repo has no `openspec/` directory. OpenSpec for all `ducky7go-mono` sub-repos lives in the parent repo: `../../openspec/` (workflow rules `../../openspec/AGENTS.md`, specs `../../openspec/specs/`, changes `../../openspec/changes/`).


# SuperDucky.Connector

## Project overview

A mod for the Unity game "Escape from Duckov", built on **Ducky.Sdk** (0.1.8, `Directory.Packages.props`). It exports an item encyclopedia (JSON metadata, multi-language descriptions, PNG icons) and records item acquisition history to files under the system temp directory, so external tools can read them. It is shipped as a NuGet package (`info.ini` metadata, packed by `ducky7go/action-ducky-nuget`). Its mod name is `SuperDucky.Connector`, license MIT.

## Repository layout

- `SuperDucky.Connector/` the only project (`SuperDucky.Connector.csproj`, references `Ducky.Sdk`)
  - `ModBehaviour.cs` entry point (`ModBehaviourBase`: `ModEnabled` / `ModDisabled`, hooks `SceneLoader.onAfterSceneInitialize`)
  - `ItemDictionaryTracker.cs` (item encyclopedia export), `ItemAcquisitionMonitor.cs` (inventory monitoring), `FileStorageService.cs` (file I/O, digit-bucket folder = `Math.Abs(itemId) % 10`), `Models/`
  - `assets/` `info.ini`, `preview.png`, `description/en.md`, `description/zh.md`
  - `Docs/data-structure.md`, `Docs/api-examples.md` documented on-disk data format for external consumers
- `scripts/` `fetch_build_dependency.sh` (downloads `Managed/` game assemblies from `ducky7go/sdk_build_dependency` releases), `setup_build_tools.sh`
- Root: `SuperDucky.Connector.slnx`, `Directory.Build.props`, `Directory.Packages.props` (central package management), `global.json` (SDK 9.0.301), `GitVersion.yml`, `nuget.config` (MyGet feed only for `Ducky.Sdk`, nuget.org for the rest)
- `../../openspec/specs/`: `superducky.connector-ci-cd`, `module-metadata`, `nuget-packaging`; `../../openspec/changes/archive/` has 4 archived changes

## Build / test / run

CI builds like this (see `.github/workflows/publish.yml` and `pr-build-validation.yml`):
1. `bash scripts/fetch_build_dependency.sh` (downloads game assemblies into `./Managed`)
2. install global tools `dotnet-script` 1.5.0 and `dotnet-ilrepack` 2.0.44 (`scripts/setup_build_tools.sh` does the same locally)
3. `CI=true dotnet build SuperDucky.Connector/SuperDucky.Connector.csproj -p:PackageVersion=<ver> -p:Configuration=Release` - output goes to `artifacts/Mods/SuperDucky.Connector/` (Ducky.Sdk uses the git root when `CI` is set, see comments in `Directory.Build.props`)

Locally, `Directory.Build.props` imports an optional `Directory.Build.local.props` and the csproj an optional `../Local.props` (git-ignored) for machine-specific game paths. There is no test project in the repo.

## CI / release

- PRs to main: `pr-build-validation.yml` builds, packs without pushing, uploads an artifact and comments on the PR.
- `publish.yml` runs on tags `*.*.*` and on pushes to main; version = tag, or GitVersion `fullSemVer` for dev builds (`GitVersion.yml`, main label `dev`). It rewrites `version =` in `info.ini` in the output folder, logs into NuGet via OIDC (`NUGET_USER` secret) and pushes only for tags/main.
- `release-drafter.yml` + `.github/release-drafter.yml`: PR labels (Breaking Change, New Feature, Feature, Bug Fix, Improvement, Docs) group release notes.

## Conventions

- Target `netstandard2.1`, `LangVersion preview`, nullable enabled, implicit usings.
- Log prefix used in code: `[DuckyConnector]`. Unity API calls on textures must stay on the main thread.
- Changing the on-disk format means updating `Docs/data-structure.md` and `Docs/api-examples.md`, since external tools depend on them.
- Mod descriptions are bilingual (`assets/description/en.md`, `zh.md`).
- Commit style: `docs(connector): ...` (conventional commits with PR number).

## Gotchas

- `info.ini` has `version = 0.1.0`; CI overwrites it at build time, so do not bump it by hand for releases.
- The build needs network access (MyGet, nuget.org, GitHub) and the `Managed/` assemblies; do not expect a plain `dotnet build` to work on a fresh clone.

## Related repositories (ducky7go)

- `DML` - the SuperDucky desktop app; it detects this mod (>= 1.0.0) for its save gallery and as a launcher dependency (`../../openspec/specs/save-gallery`, `game-convenience`).
- `SuperDucky.docs` - user docs for the app (has an `enable-save-gallery` guide, which does not name this mod).
- `Ducky.Sdk` and `sdk_build_dependency` - SDK and game assemblies this mod builds against; `action-ducky-nuget` packs it.
