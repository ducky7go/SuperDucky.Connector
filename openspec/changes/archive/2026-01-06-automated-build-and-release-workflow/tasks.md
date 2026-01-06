# Automated Build and Release Workflow - Implementation Tasks

## 1. Foundation Setup

- [x] 1.1 Create `global.json` with .NET SDK version (9.0.301) for GitHub Actions build consistency
- [x] 1.2 Create `GitVersion.yml` configuration with Mainline mode and `next-version: 1.0.0`
- [ ] 1.3 Add `GitVersion.Tool` to local dev environment (`dotnet tool install --global GitVersion.Tool`) (manual)
- [ ] 1.4 Validate GitVersion locally by running `dotnet-gitversion` and verifying output (manual)
- [x] 1.5 Create `.github/` directory structure if not exists
- [x] 1.6 Create `.github/workflows/` directory

## 2. Release Drafter Workflow

- [x] 2.1 Create `.github/workflows/release-drafter.yml` based on Ducky.Sdk reference
- [x] 2.2 Configure workflow triggers (push to main, pull requests)
- [x] 2.3 Set required permissions (contents: write, pull-requests: write)
- [x] 2.4 Create `.github/release-drafter.yml` configuration with categories matching Ducky.Sdk
- [ ] 2.5 Test workflow by merging a PR to main and verifying draft release creation (manual)

## 3. Build and Publish Workflow

- [x] 3.1 Create `.github/workflows/publish.yml` based on Ducky.Sdk reference
- [x] 3.2 Configure workflow triggers (tags `*.*.*`, main branch pushes)
- [x] 3.3 Add checkout step with `fetch-depth: 0` for GitVersion
- [x] 3.4 Add `actions/setup-dotnet@v4` step with global.json reference
- [x] 3.5 Add NuGet package caching step using `actions/cache@v4`
- [x] 3.6 Add GitVersion setup step (`gittools/actions-gitversion/setup@v4.2.0`)
- [x] 3.7 Add GitVersion execute step with config file path
- [x] 3.8 Add "Determine Version" step with tag vs branch logic
- [x] 3.9 Add "Build and Pack" step using `dotnet pack` with version parameter
- [x] 3.10 Add "Upload Artifacts" step with 30-day retention
- [x] 3.11 Add "Push to NuGet" step with secret-based authentication
- [x] 3.12 Configure `continue-on-error: true` for optional MyGet publishing

## 4. Project Configuration Updates

- [x] 4.1 Remove MinVer PackageReference from `Directory.Packages.props`
- [x] 4.2 Add GitVersion MSBuild import to `Directory.Build.props` (conditional on file existence)
- [x] 4.3 Validate build locally with `dotnet build` and `dotnet pack`
- [ ] 4.4 Verify generated NuGet package version matches GitVersion output (manual)

## 5. GitHub Secrets Configuration

- [x] 5.1 Document required secrets in project.md
- [ ] 5.2 Add `NUGET_KEY` to repository Secrets (via GitHub UI) (manual)
- [ ] 5.3 (Optional) Add `MYGET_API_KEY` to repository Secrets (manual)
- [ ] 5.4 Validate secret access in test workflow run (manual)

## 6. Validation and Testing

- [ ] 6.1 Test tag-based release: Create and push a test tag (e.g., `v1.0.0-test`) (manual)
- [ ] 6.2 Verify workflow triggers and completes successfully (manual)
- [ ] 6.3 Verify NuGet package appears in artifacts (manual)
- [ ] 6.4 Verify NuGet package published to NuGet.org (if key configured) (manual)
- [ ] 6.5 Test main branch build: Push commit to main branch (manual)
- [ ] 6.6 Verify dev build version format (X.Y.Z-dev.N) (manual)
- [ ] 6.7 Verify Release Drafter updated draft release notes (manual)
- [ ] 6.8 Clean up test tag if needed: `git tag -d v1.0.0-test && git push origin :refs/tags/v1.0.0-test` (manual)

## 7. Documentation

- [x] 7.1 Add "Release Process" section to project.md explaining tag-based releases
- [x] 7.2 Add "CI/CD" section documenting GitHub Actions workflows
- [x] 7.3 Add "Secrets Setup" guide for contributors setting up fork repositories
- [x] 7.4 Add "Troubleshooting" section for common workflow failures (documented in CI/CD section)
- [x] 7.5 Update openspec/project.md "Build Dependencies" to reflect GitVersion instead of MinVer

## 8. Post-Migration Cleanup

- [x] 8.1 Verify no MinVer references remain in project files
- [x] 8.2 Verify GitVersion.yml configuration matches intended strategy
- [x] 8.3 Run `openspec validate automated-build-and-release-workflow --strict`
- [x] 8.4 Confirm all tasks completed before marking proposal as implemented
