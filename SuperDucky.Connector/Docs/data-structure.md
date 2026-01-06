# Data Structure Documentation

This document explains how the SuperDucky.Connector mod saves data, for external programs to integrate based on folder data.

## Base Directory

All data is stored in the system temp directory under `SuperDucky.Connector`:

- **Windows**: `C:\Users\{username}\AppData\Local\Temp\SuperDucky.Connector\`
- **Linux/macOS**: `/tmp/SuperDucky.Connector/`

## Folder Structure

```
SuperDucky.Connector/
├── items/                    # Item encyclopedia data
│   └── {save_slot}/          # Per-save-slot separation
│       ├── 0/                # Items ending with digit 0
│       ├── 1/                # Items ending with digit 1
│       ├── ...
│       └── 9/                # Items ending with digit 9
│           └── {item_id}/    # Individual item folder
│               ├── metadata.json      # Item properties
│               ├── description.json   # Multi-language descriptions
│               ├── preview.png        # Item icon (64x64)
│               └── .last_update       # Timestamp for change detection
└── history/                  # Item acquisition history
    └── {save_slot}/          # Per-save-slot separation
        ├── history_20250105_143022.json  # Time-sharded batches
        ├── history_20250105_143158.json
        └── ...
```

## Item Data Structure

### metadata.json

Contains all item properties from the game's ItemStatsSystem.Item class.

```json
{
  "itemId": 1001,
  "displayName": "Bandage",
  "displayNameKey": "Item_Bandage_Name",
  "descriptionKey": "Item_Bandage_Desc",
  "stackable": true,
  "maxStackCount": 10,
  "weight": 0.1,
  "price": 50,
  "displayQuality": "Common",
  "tags": ["Medical", "Consumable"],
  "stats": [
    {"key": "HealthRestore", "value": 20}
  ],
  "order": 1000,
  "lastModified": "2025-01-05T14:30:22Z"
}
```

### description.json

Multi-language support for item descriptions.

```json
{
  "itemId": 1001,
  "descriptions": {
    "en": {
      "name": "Bandage",
      "description": "Restores a small amount of health."
    },
    "zh": {
      "name": "绷带",
      "description": "恢复少量生命值。"
    }
  }
}
```

### preview.png

- **Format**: PNG
- **Dimensions**: Typically 64x64 pixels (varies by item)
- **Source**: Item sprite from game assets
- **Note**: Some items may not have exportable images due to texture read restrictions

## History Data Structure

### history_{timestamp}.json

Time-sharded append-only files recording item acquisition events. Each file represents a batch of items acquired around the same time.

```json
{
  "version": "1.0.0",
  "timestamp": "2025-01-05T14:30:22Z",
  "items": [1001, 1002, 1001],
  "quantities": [5, 1, 2]
}
```

**Field Explanation**:
- `version`: Schema version for data format compatibility
- `timestamp`: When this batch was recorded (UTC)
- `items`: List of item IDs acquired (may contain duplicates if same item acquired multiple times)
- `quantities`: Corresponding stack counts for each item entry

**Example Interpretation**:
- The above example shows acquiring 5x Bandage (1001), 1x Metal (1002), and 2x more Bandage (1001)
- For encyclopedia purposes: Items 1001 and 1002 are marked as "collected"

## Save Slot Detection

Save slot detection uses the game's `Saves.SavesSystem.CurrentSlot` property, which reads from PlayerPrefs with key "CurrentSlot". The default value is 1 (save slots are 1-indexed).

External programs should handle the `{save_slot}` folder structure flexibly:
- Read from the current slot folder
- Be prepared for multiple save slots (1, 2, 3, etc.)

## Change Detection

Items are only re-exported when their properties change. Change detection uses:
- `.last_update` file timestamp
- Comparison with current item properties from game

This minimizes I/O and ensures efficient updates.

## External Integration Guide

### Reading Item Encyclopedia

1. Scan all digit folders (0-9) under `items/{save_slot}/`
2. For each item folder, read `metadata.json` and `description.json`
3. Load `preview.png` if visual display is needed
4. Use `itemId` as the primary key for cross-referencing

### Reading Acquisition History

1. List all `history_*.json` files under `history/{save_slot}/`
2. Sort by filename (which contains timestamp)
3. Parse each file and collect all unique item IDs
4. Match `items` array with encyclopedia data using `itemId`

### Monitoring for Updates

1. Watch for new `history_*.json` files (real-time acquisition events)
2. Watch for `.last_update` file changes (encyclopedia updates)
3. Re-read affected files on change

## Data Separation Rationale

- **Items vs History**: Separated for efficient access patterns
  - Encyclopedia data (items) is read-heavy, updated infrequently
  - History data is append-only, updated frequently during gameplay

- **Time-Sharded History**: Enables read-write separation
  - New acquisitions create new files (no file locking issues)
  - External readers can safely read old files while game writes new ones

- **Per-Save Separation**: Different game saves have independent progress
  - Prevents data contamination between save slots
  - Enables save-specific analysis
