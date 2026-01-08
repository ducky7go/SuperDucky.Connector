# nuget-packaging Specification

## Purpose
TBD - created by archiving change nuget-package-metadata-enhancement. Update Purpose after archive.
## Requirements
### Requirement: Info.ini Basic Metadata

The system SHALL provide basic metadata fields in the `info.ini` file for mod identification.

#### Scenario: Basic fields present
- **WHEN** the `info.ini` file is read
- **THEN** it contains `name`, `displayName`, `description`, and `version` fields

#### Scenario: Version format
- **WHEN** the `version` field is read
- **THEN** it follows semantic versioning format (X.Y.Z)

### Requirement: Info.ini Extended Metadata

The system SHALL provide extended metadata fields in the `info.ini` file for NuGet package generation.

#### Scenario: Author information
- **WHEN** the `info.ini` file is read
- **THEN** it contains `author` and `authors` fields identifying the package creator

#### Scenario: License declaration
- **WHEN** the `info.ini` file is read
- **THEN** it contains a `license` field with a valid SPDX license expression (e.g., MIT, Apache-2.0)

#### Scenario: Project URL
- **WHEN** the `info.ini` file is read
- **THEN** it contains `homepage` and `projectUrl` fields linking to the project repository or documentation

#### Scenario: Repository information
- **WHEN** the `info.ini` file is read
- **THEN** it contains `repository.type` and `repository.url` fields for source code tracking

#### Scenario: Package tags
- **WHEN** the `info.ini` file is read
- **THEN** it contains a `tags` field with space-separated keywords for package discoverability

### Requirement: Nuspec Mapping Compliance

The system SHALL map `info.ini` fields to `.nuspec` elements according to NuGet Mod Packaging Specification v1.0.

#### Scenario: Authors mapping
- **WHEN** generating a `.nuspec` file
- **THEN** the `authors` field from `info.ini` maps to the `<authors>` element

#### Scenario: License mapping
- **WHEN** generating a `.nuspec` file
- **THEN** the `license` field from `info.ini` maps to the `<license type="expression">` element

#### Scenario: Project URL mapping
- **WHEN** generating a `.nuspec` file
- **THEN** the `projectUrl` field from `info.ini` maps to the `<projectUrl>` element

#### Scenario: Tags mapping
- **WHEN** generating a `.nuspec` file
- **THEN** the `tags` field from `info.ini` maps to the `<tags>` element

