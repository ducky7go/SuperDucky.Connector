# Add Module Description Files

## Overview

Add bilingual (English and Chinese) module description files to the `assets` directory to comply with module publishing standards. This change enables users and external tooling to understand the core functionality and integration points of the SuperDucky.Connector mod.

## Status

**ExecutionCompleted** | [OpenSpec Change ID: `module-description-files-arg-value`]

## Context

### Background

The `SuperDucky.Connector` mod currently lacks standardized description files in its `assets` directory. According to module publishing conventions, mods should provide bilingual (English and Chinese) detailed functional descriptions for:
- User understanding of mod capabilities
- External tool compatibility (module loader metadata reading)
- Community accessibility (Chinese and English user bases)

### Current State

The `SuperDucky.Connector/assets/` directory currently contains:
- `info.ini` - Basic mod metadata (name, version, author, repository)
- `preview.png` - Mod preview image

**Missing**: `assets/description/en.md` and `assets/description/zh.md` files

### Problem Statement

1. **Missing English description file** - No `assets/description/en.md` exists
2. **Missing Chinese description file** - No `assets/description/zh.md` exists
3. **Missing description directory** - No `assets/description/` folder exists
4. **Incomplete documentation** - No comprehensive functional description for users or tools

## Scope

### In Scope

1. Create `assets/description/` directory
2. Create `en.md` with English game feature description
3. Create `zh.md` with Chinese game feature description
4. Focus on in-game functionality: what the mod does in the game context
5. Describe player-visible features: item encyclopedia, collection tracking

### Out of Scope

- Modifying existing code functionality
- Changing the `info.ini` format
- Altering data storage structures
- Updating API documentation (already exists in `Docs/`)

## Design

### File Structure

```
SuperDucky.Connector/assets/
├── info.ini              # Existing: Basic metadata
├── preview.png           # Existing: Mod preview
└── description/          # NEW: Description folder
    ├── en.md             # NEW: English description
    └── zh.md             # NEW: Chinese description
```

### Content Requirements

**Each description file must include:**

1. **Game Features**
   - Item encyclopedia export: Automatically exports all game items with names, descriptions, and icons
   - Acquisition monitoring: Tracks items collected by the player in real-time
   - Per-save tracking: Separates data for each save slot

2. **How It Works**
   - Runs automatically when the game loads
   - Exports item data to files in system temp directory
   - Updates continuously as you collect new items

## Impact Assessment

### Benefits

| Area | Impact |
|------|--------|
| **User Experience** | Standardized description files enable quick understanding of mod functionality |
| **Tool Compatibility** | Complies with module loader metadata reading conventions |
| **Community** | Bilingual documentation supports Chinese and English user bases |
| **Discoverability** | External tools can parse descriptions for mod catalogs |

### Risks

| Risk | Mitigation |
|------|------------|
| Content accuracy | Use existing `Docs/data-structure.md` and code as source of truth |
| Translation quality | Review both English and Chinese versions for consistency |

### Non-Impact

- No code changes required
- No runtime behavior changes
- No data format changes
- No API changes

## Dependencies

None. This is a documentation-only change with no code dependencies.

## Success Criteria

1. `assets/description/` directory exists
2. `assets/description/en.md` exists
3. `assets/description/zh.md` exists
4. Both files use lowercase naming convention
5. Content focuses on game features (item encyclopedia, collection tracking)
6. Descriptions are player-friendly and understandable
7. Both files are valid Markdown

## Alternatives Considered

### Alternative 1: Single description file
**Rejected**: Bilingual support is required for the project's Chinese and English user bases.

### Alternative 2: Keep existing `info.ini` description only
**Rejected**: The `info.ini` `description` field is too brief (current value is just the display name). Detailed markdown files provide the depth needed for users and tools.

## References

- Data structure documentation: `SuperDucky.Connector/Docs/data-structure.md`
- API examples: `SuperDucky.Connector/Docs/api-examples.md`
- Project conventions: `openspec/project.md`

## Related Changes

None. This is an independent documentation enhancement.
