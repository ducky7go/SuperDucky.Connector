# Project Context

## Purpose
SuperDucky.Connector is a game mod for "Escape from Duckov" that exports comprehensive item encyclopedia data and acquisition history for external analysis and integration.

**Current Status**: Active development - core functionality implemented with item data export, real-time acquisition monitoring, and file-based storage architecture.

## Tech Stack
**Core Technologies**:
- **.NET Standard 2.1** / C# - Mod framework targeting
- **Ducky.Sdk** (0.1.7-dev.2) - Game modding SDK
- **Lib.Harmony** (2.4.1) - Runtime patching for game method interception
- **Unity Engine** - Game engine integration (Texture2D, Sprite, SceneLoader)

**Data & Storage**:
- **JSON** - Item metadata and history serialization (Newtonsoft.Json via Ducky.Sdk)
- **PNG** - Item icon/image export
- **File-based storage** - Time-sharded append-only architecture

**Async & Concurrency**:
- **UniTask** - Async/await integration with Unity main thread
- **System.Threading.Channels** - Async data pipelines
- **R3** (3.3.4) - Reactive extensions for event streams

**Development Tools**:
- Git for version control
- OpenSpec CLI for specification-driven development
- Claude Code with OpenSpec integration
- MCP servers for extended capabilities
- GitHub Actions for CI/CD automation

**Testing Framework**:
- NUnit (4.3.2) - Unit testing
- NSubstitute (5.3.0) - Mocking framework
- Shouldly (4.3.0) - Assertion library

## Project Conventions

### Code Style
**Language Preferences**:
- Documentation: Bilingual (English for technical specs, Chinese for design guidelines)
- Code comments: English (follow .NET conventions with XML doc comments)
- Commit messages: English (conventional commits format)
- Public API documentation: English (see Docs/ folder)

**Formatting**:
- C# conventions: PascalCase for public members, _camelCase for private fields
- XML documentation comments on all public APIs
- Tabs for indentation (C# standard)
- File naming: PascalCase matching class names

### Architecture Patterns
**Design Philosophy**:
- **Simplicity First**: Single-responsibility components, file-scoped implementations
- **Proven Patterns**: File-based storage for read-write separation, event-driven monitoring
- **Separation of Concerns**: Storage service isolated from tracking/monitoring logic
- **Harmony Patching**: Minimal and targeted patches for game event interception

**Mod Architecture**:
```
ModBehaviour (Entry Point)
├── FileStorageService (I/O layer)
├── ItemDictionaryTracker (Encyclopedia collection)
└── ItemAcquisitionMonitor (Real-time event monitoring)
```

**Documentation-Driven Development**:
- Use OpenSpec for all non-trivial changes (features, breaking changes, architecture shifts)
- Include visual diagrams (Mermaid, ASCII art) for data flow changes
- Maintain spec-to-code alignment through validation
- Bilingual documentation: English technical specs, Chinese design guidelines

### Testing Strategy
**Approach**:
- Unit tests for isolated components (FileStorageService, data models)
- Integration tests for storage operations
- Manual testing in-game for Harmony patches and event subscriptions

**Validation**:
- Run `openspec validate [change] --strict` before implementation
- Ensure all tasks.md items are completed before marking done
- Verify file output format matches documented schema

### Git Workflow
**Branching Strategy**:
- `main`: Production-ready code
- Feature branches: `change-id` format matching OpenSpec change ID
- Direct fixes: Bug fixes, typos (without proposals)

**Commit Conventions**:
- Use `openspec apply` for implementing approved proposals
- Reference change IDs in commits: `apply: add-feature-name`
- Archive changes after deployment: `openspec archive <change-id>`

### CI/CD Pipeline
**GitHub Actions Workflows**:
- `.github/workflows/release-drafter.yml` - Automatic release notes generation
- `.github/workflows/publish.yml` - NuGet package build and publish

**Release Process**:
1. Create and push version tag: `git tag v1.0.0 && git push origin v1.0.0`
2. Release Drafter automatically generates release notes from commit history
3. Build workflow triggers on tag push:
   - GitVersion calculates semantic version
   - `dotnet pack` creates NuGet package
   - Package uploaded as artifact (30-day retention)
   - Published to NuGet.org via `NUGET_KEY` secret
4. Main branch pushes generate dev builds (X.Y.Z-dev.N) without publishing

**Secrets Configuration**:
- `NUGET_KEY` - NuGet.org API key (required for publishing)
- `MYGET_API_KEY` - (Optional) MyGet feed API key

**Version Management**:
- GitVersion (Mainline mode) replaces MinVer for semantic versioning
- Tag format: `v1.2.3` (formal releases) or `1.2.3`
- Dev builds: Auto-increment based on commits since last tag

## Domain Context
**Project Domain**: Game mod for data extraction and external integration

**Target Game**: Escape from Duckov (Unity-based)
**Mod Type**: BepInEx plugin using Ducky.Sdk framework

**Key Concepts**:
- **Item Encyclopedia**: Complete export of all game items with metadata, descriptions, and images
- **Acquisition Monitoring**: Real-time tracking of player inventory changes
- **Event-Driven Architecture**: Reacts to game scene loads and inventory state changes
- **File-Based API**: External programs read exported JSON/PNG files for integration
- **Time-Sharded Storage**: Append-only history files prevent write conflicts

**Data Flow**:
1. Game loads → Mod initializes
2. Main scene loads → Start item collection and monitoring
3. Player acquires items → Event triggered → Batch recorded to file
4. External programs read files → Display/analyze game data

**Assumptions**:
- Game runs with BepInEx mod framework installed
- File system access available to temp directory
- Unity main thread accessible for texture operations
- Multiple save slots require data separation

## Important Constraints
**Technical Constraints**:
- Must run on .NET Standard 2.1 (Unity compatibility)
- Unity API calls (texture operations) must execute on main thread
- File I/O must use async/await to prevent game frame drops
- Harmony patches must be reversible and minimal

**Development Constraints**:
- All non-trivial changes require OpenSpec proposals
- Visual documentation required for data flow changes
- Validation must pass before implementation starts
- Public APIs must have XML documentation

**Operational Constraints**:
- Zero game performance impact (async operations, background threads)
- Graceful degradation if file operations fail
- Thread-safe operations for concurrent read/write
- Per-save-slot data isolation

**File System Constraints**:
- Uses system temp directory (platform-dependent)
- Digit-based folder distribution (0-9) for item organization
- Time-sharded history files (format: `history_yyyyMMdd_HHmmss.json`)

## External Dependencies
**Runtime Dependencies**:
- **Ducky.Sdk** - Game modding framework and utilities
- **Lib.Harmony** - Runtime method patching for event interception
- **Newtonsoft.Json** (via SDK) - JSON serialization
- **UniTask** (via SDK) - Unity-compatible async operations

**Development Dependencies**:
- OpenSpec CLI (for specification management)
- Claude Code (AI-assisted development)
- MCP servers: shadcn, web-reader, web-search-prime, 4.5v-mcp, zai-mcp-server

**Build Dependencies**:
- .NET SDK 9.0.301+ (for compilation)
- MSBuild (Microsoft.Build 17.9.0)
- GitVersion (6.x) - Semantic versioning for CI/CD

**Integration Points**:
- Game's `ItemAssetsCollection` - Source of item encyclopedia data
- Game's `SceneLoader` - Scene change events
- Game's inventory system - Acquisition events via Harmony patches
- File system - Data export to temp directory

## Documentation Standards
**Visual Documentation**:
- Use ASCII art for file structure layouts
- Use Mermaid diagrams for data flows, component relationships
- Include code change tables for file-level tracking
- Reference: `openspec/PROPOSAL_DESIGN_GUIDELINES.md` (Chinese)

**Language Guidelines**:
- Technical specs: English (`openspec/AGENTS.md`)
- Design guidelines: Chinese (`openspec/PROPOSAL_DESIGN_GUIDELINES.md`)
- API documentation: English (`SuperDucky.Connector/Docs/`)
- Code comments: English (XML doc format)

**Structure**:
- `openspec/specs/` - Current truth (what IS built)
- `openspec/changes/` - Proposals (what SHOULD change)
- `openspec/changes/archive/` - Completed changes
- `SuperDucky.Connector/Docs/` - API and data structure documentation

## Architecture Overview

### Core Components

**ModBehaviour** (`SuperDucky.Connector/ModBehaviour.cs:9`)
- Main mod entry point inheriting from `ModBehaviourBase`
- Initializes all services on mod load
- Coordinates scene change events
- Manages service lifecycle and cleanup

**FileStorageService** (`SuperDucky.Connector/FileStorageService.cs:9`)
- Separated storage architecture with digit-based folder distribution
- Async file I/O operations for metadata, descriptions, and images
- Time-sharded history batch writing
- Handles Unity texture conversion and PNG encoding

**ItemDictionaryTracker** (`SuperDucky.Connector/ItemDictionaryTracker.cs`)
- Collects complete item encyclopedia from `ItemAssetsCollection`
- Change detection using timestamps (FirstSeenAt, LastUpdatedAt)
- Background thread scanning with UniTask
- Progress logging and error handling

**ItemAcquisitionMonitor** (`SuperDucky.Connector/ItemAcquisitionMonitor.cs`)
- Real-time inventory monitoring via Harmony-patched events
- Debounced batch recording (300ms delay)
- Per-save-slot tracking
- Initialization with existing inventory items

### Data Models

**ItemMetadata** (`SuperDucky.Connector/Models/ItemMetadata.cs:7`)
- Complete item properties from game's `Item` class
- Includes: ID, display names, stackability, weight, value, quality, tags, stats, durability, timestamps
- Serialized to JSON in `metadata.json`

**ItemDescription** (`SuperDucky.Connector/Models/ItemDescription.cs`)
- Multi-language support for item names and descriptions
- Language-keyed dictionary structure

**AcquisitionBatch** (`SuperDucky.Connector/Models/AcquisitionBatch.cs`)
- Time-stamped batch of acquired items
- Parallel arrays for item IDs and quantities

### File Organization

**Storage Structure**:
```
{TEMP}/SuperDucky.Connector/Data/
├── items/{save_slot}/{0-9}/{item_id}/
│   ├── metadata.json       # Item properties
│   ├── description.json    # Multi-language descriptions
│   └── preview.png         # Item icon
└── history/{save_slot}/
    └── history_{yyyyMMdd_HHmmss}.json  # Acquisition batches
```

**Documentation Structure**:
```
SuperDucky.Connector/
├── Docs/
│   ├── api-examples.md      # Usage examples (C#, Python, Node.js)
│   └── data-structure.md    # Schema documentation
└── assets/
    └── info.ini             # Mod metadata
```

## Quick Reference

**OpenSpec Commands**:
```bash
openspec list                  # List active changes
openspec list --specs          # List specifications
openspec show [item]           # Display change or spec details
openspec validate [change] --strict  # Validate changes
openspec archive <change-id> --yes   # Archive after deployment
```

**Build Commands**:
```bash
dotnet build                   # Build the mod
dotnet test                    # Run unit tests
```

**File References**:
- Agent instructions: `openspec/AGENTS.md`
- Design guidelines: `openspec/PROPOSAL_DESIGN_GUIDELINES.md` (Chinese)
- Project conventions: `openspec/project.md` (this file)
- API documentation: `SuperDucky.Connector/Docs/api-examples.md`
- Data schema: `SuperDucky.Connector/Docs/data-structure.md`

**Change Workflow**:
1. Create proposal for new features/breaking changes
2. Validate with `openspec validate --strict`
3. Implement following tasks.md checklist
4. Archive after deployment with `openspec archive`

**Key Implementation Details**:
- Digit folder calculation: `Math.Abs(itemId) % 10`
- Save slot detection: `Saves.SavesSystem.CurrentSlot`
- Texture conversion: RenderTexture blit for non-readable textures
- Debounce delay: 300ms for acquisition batching

**Remember**: Specs are truth. Changes are proposals. Keep them in sync.
