# ci-cd Specification

## Purpose
TBD - created by archiving change automated-build-and-release-workflow. Update Purpose after archive.
## Requirements
### Requirement: Automated Release Note Generation

The system SHALL automatically generate and maintain release notes using GitHub Actions Release Drafter when commits are pushed to the main branch or pull requests are merged.

#### Scenario: Draft release created on merge
- **WHEN** a pull request is merged to the main branch with a conventional commit prefix (e.g., "feat:", "fix:")
- **THEN** the Release Drafter workflow updates the draft release notes with the new entry categorized by commit type

#### Scenario: Draft release updated on main branch push
- **WHEN** commits are pushed directly to the main branch
- **THEN** the Release Drafter workflow updates the draft release notes to include the new commits

#### Scenario: Commit categorization
- **WHEN** commits follow conventional commit format
- **THEN** releases notes categorize entries as "Features", "Bug Fixes", "Documentation", or "Performance Improvements" based on commit prefix

### Requirement: Automated Build and Packaging

The system SHALL automatically build and package NuGet artifacts on every push to the main branch and on version tags.

#### Scenario: Tag-based build for formal release
- **WHEN** a semantic version tag (X.Y.Z format) is pushed to the repository
- **THEN** the build workflow compiles the project, packages it as a NuGet package using the tag version, and uploads artifacts

#### Scenario: Main branch build for dev version
- **WHEN** a commit is pushed to the main branch without a tag
- **THEN** the build workflow compiles the project, packages it using GitVersion-calculated dev version (X.Y.Z-dev.N), and uploads artifacts

#### Scenario: Build artifact retention
- **WHEN** the build workflow completes successfully
- **THEN** generated NuGet packages are uploaded as GitHub artifacts with 30-day retention

### Requirement: Semantic Version Calculation

The system SHALL calculate semantic versions using GitVersion based on git history and branch configuration.

#### Scenario: Formal release version from tag
- **WHEN** a semantic version tag (e.g., `v1.2.3` or `1.2.3`) exists on the commit
- **THEN** the system uses the tag version directly for packaging (output: `1.2.3`)

#### Scenario: Development version from main branch
- **WHEN** building on the main branch after the last tag
- **THEN** GitVersion calculates the next patch version with dev build suffix (output: `1.2.4-dev.5` where 5 is commits since last tag)

#### Scenario: Version configuration
- **WHEN** GitVersion.yml is configured with Mainline mode and `next-version: 1.0.0`
- **THEN** version calculation respects branch configuration and increments appropriately (Patch for main branch)

### Requirement: Automated NuGet Publishing

The system SHALL automatically publish NuGet packages to NuGet.org when version tags are pushed.

#### Scenario: Formal release publishing
- **WHEN** a semantic version tag (X.Y.Z) is pushed and the build completes successfully
- **THEN** the workflow pushes the generated NuGet package to NuGet.org using the configured API key

#### Scenario: Publishing authentication
- **WHEN** the publish step executes
- **THEN** the workflow uses the `NUGET_KEY` GitHub Secret for authentication to NuGet.org

#### Scenario: Duplicate package handling
- **WHEN** a package version already exists on NuGet.org
- **THEN** the publish step uses `--skip-duplicate` flag to fail gracefully without blocking the workflow

#### Scenario: Optional dev build publishing
- **WHEN** a main branch build produces a dev version (X.Y-Z-dev.N)
- **THEN** publishing to NuGet.org is optional (configured via `continue-on-error: true` or workflow input)

### Requirement: GitHub Actions Workflow Configuration

The system SHALL provide GitHub Actions workflows for release automation with proper permissions and caching.

#### Scenario: Workflow permissions
- **WHEN** workflows are defined in `.github/workflows/`
- **THEN** the Release Drafter workflow has `contents: write` and `pull-requests: write` permissions, and the publish workflow has default `contents: read` permissions

#### Scenario: NuGet package caching
- **WHEN** the build workflow runs
- **THEN** NuGet packages are cached using `actions/cache@v4` with key based on `Directory.Packages.props` hash to improve build times

#### Scenario: .NET SDK version pinning
- **WHEN** the build workflow executes
- **THEN** `actions/setup-dotnet@v4` uses `global.json` to ensure consistent .NET SDK version across builds

### Requirement: Version Migration Support

The system SHALL support migration from MinVer to GitVersion without breaking existing build processes.

#### Scenario: GitVersion configuration file
- **WHEN** `GitVersion.yml` exists in repository root
- **THEN** GitVersion uses the configuration instead of default settings

#### Scenario: MSBuild integration
- **WHEN** the project builds with MSBuild via `dotnet build` or `dotnet pack`
- **THEN** GitVersion injects version properties into the build process (replacing MinVer functionality)

#### Scenario: Local development validation
- **WHEN** a developer runs `dotnet-gitversion` locally
- **THEN** the output matches what the CI/CD workflow will calculate, allowing pre-commit validation

### Requirement: Documentation for CI/CD Setup

The system SHALL provide documentation for setting up and troubleshooting the automated build and release workflow.

#### Scenario: Secret setup instructions
- **WHEN** a contributor reviews the project documentation
- **THEN** instructions are provided for adding `NUGET_KEY` to GitHub Secrets

#### Scenario: Release process documentation
- **WHEN** a maintainer needs to create a formal release
- **THEN** documentation explains that pushing a semantic version tag triggers automated build and publish

#### Scenario: Troubleshooting guide
- **WHEN** a workflow fails
- **THEN** documentation includes common failure scenarios and resolution steps (e.g., secret misconfiguration, version calculation errors)

