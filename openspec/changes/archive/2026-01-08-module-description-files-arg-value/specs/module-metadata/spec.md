## ADDED Requirements

### Requirement: Module Description Files

The mod SHALL provide bilingual description files in the `assets/description/` directory to describe game features in a player-friendly manner.

#### Scenario: Description directory exists
- **GIVEN** the mod is properly installed
- **WHEN** listing directories in `assets/`
- **THEN** `description/` directory SHALL exist

#### Scenario: Description files exist
- **GIVEN** the `description/` directory exists
- **WHEN** listing files in the `assets/description/` directory
- **THEN** `en.md` SHALL exist
- **AND** `zh.md` SHALL exist

#### Scenario: Filename follows naming convention
- **GIVEN** description files are created
- **WHEN** examining filenames
- **THEN** English file SHALL be named `en.md`
- **AND** Chinese file SHALL be named `zh.md`
- **AND** all filenames SHALL use lowercase letters only

#### Scenario: Content describes game features
- **GIVEN** a description file is read
- **WHEN** parsing the content
- **THEN** the file SHALL describe what the mod does in the game
- **AND** SHALL include item encyclopedia feature (exports all game items)
- **AND** SHALL include acquisition monitoring feature (tracks collected items)
- **AND** SHALL include per-save tracking feature (separate data per save slot)

#### Scenario: Content explains how it works
- **GIVEN** a description file is read
- **WHEN** parsing the content
- **THEN** the file SHALL explain it runs automatically when game loads
- **AND** SHALL mention data is saved to temp directory
- **AND** SHALL be understandable by players

#### Scenario: Content is player-friendly
- **GIVEN** a description file is read
- **WHEN** evaluating the content
- **THEN** the content SHALL avoid technical implementation details
- **AND** SHALL focus on player-visible features
- **AND** SHALL be concise and easy to understand

#### Scenario: Bilingual content consistency
- **GIVEN** both `en.md` and `zh.md` exist
- **WHEN** comparing the content
- **THEN** both files SHALL describe the same game features
- **AND** both SHALL be player-friendly
- **AND** all features SHALL be present in both languages

#### Scenario: Files are valid Markdown
- **GIVEN** description files exist
- **WHEN** validating file format
- **THEN** both files SHALL be valid Markdown format
- **AND** both files SHALL use UTF-8 encoding
