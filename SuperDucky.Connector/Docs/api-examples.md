# SuperDucky.Connector API

A mod for "Escape from Duckov" that exports game item encyclopedia data and acquisition history for external analysis.

## Overview

SuperDucky.Connector runs as a background service that:
1. Collects all item data from the game's ItemAssetsCollection
2. Exports item metadata, descriptions, and images to JSON/PNG files
3. Monitors inventory changes in real-time
4. Records item acquisition history

All data is stored in the system temp directory for external program integration.

## Installation

Place the `SuperDucky.Connector` folder in:
```
{GameInstallDirectory}/BepInEx/plugins/SuperDucky.Connector/
```

## File Locations

### Base Directory
- **Windows**: `%TEMP%\SuperDucky.Connector\`
- **Linux/macOS**: `/tmp/SuperDucky.Connector/`

### Data Organization
```
SuperDucky.Connector/
├── items/{save_slot}/{0-9}/{item_id}/
│   ├── metadata.json      # Item properties
│   ├── description.json   # Multi-language descriptions
│   └── preview.png        # Item icon
└── history/{save_slot}/
    └── history_*.json     # Acquisition batches
```

See [data-structure.md](./data-structure.md) for detailed schema documentation.

## Usage Examples

### Reading Item Encyclopedia (C#)

```csharp
using System.IO;
using Newtonsoft.Json;

var basePath = Path.Combine(Path.GetTempPath(), "SuperDucky.Connector");
var itemsPath = Path.Combine(basePath, "items", "0");

// Read item metadata
var metadataPath = Path.Combine(itemsPath, "1001", "metadata.json");
var metadataJson = await File.ReadAllTextAsync(metadataPath);
var metadata = JsonConvert.DeserializeObject<ItemMetadata>(metadataJson);

Console.WriteLine($"Item: {metadata.DisplayName}, Weight: {metadata.Weight}");
```

### Reading Acquisition History (Python)

```python
import json
from pathlib import Path
from datetime import datetime

base_path = Path("/tmp/SuperDucky.Connector")
history_path = base_path / "history" / "0"

# Get all history files sorted by timestamp
history_files = sorted(history_path.glob("history_*.json"))

for hist_file in history_files:
    with open(hist_file, 'r') as f:
        batch = json.load(f)
        timestamp = datetime.fromisoformat(batch['timestamp'].replace('Z', '+00:00'))

        for item_id, quantity in zip(batch['items'], batch['quantities']):
            print(f"{timestamp}: Acquired {quantity}x Item {item_id}")
```

### Monitoring Real-Time Updates (Node.js)

```javascript
const fs = require('fs');
const path = require('path');
const chokidar = require('chokidar');

const basePath = path.join(require('os').tmpdir(), 'SuperDucky.Connector');
const historyPath = path.join(basePath, 'history', '0');

// Watch for new history files
const watcher = chokidar.watch(`${historyPath}/history_*.json`, {
  persistent: true,
  ignoreInitial: true
});

watcher.on('add', (filePath) => {
  const batch = JSON.parse(fs.readFileSync(filePath, 'utf8'));
  console.log(`New acquisition batch at ${batch.timestamp}`);

  batch.items.forEach((itemId, index) => {
    const quantity = batch.quantities[index];
    console.log(`  - ${quantity}x Item ${itemId}`);
  });
});
```

## Mod Components

### Core Services

#### FileStorageService
Handles all file I/O operations with separated storage architecture.

```csharp
var storageService = new FileStorageService();

// Write item metadata
await storageService.WriteItemMetadataAsync(itemId, saveSlot, metadata);

// Write acquisition batch
await storageService.WriteHistoryBatchAsync(saveSlot, batch);

// Read item metadata
var metadata = await storageService.ReadItemMetadataAsync(itemId, saveSlot);
```

#### ItemDictionaryTracker
Collects and exports item encyclopedia data from game assets.

```csharp
var tracker = new ItemDictionaryTracker(storageService);
tracker.StartCollectionAsync(); // Runs on background thread
```

**Features**:
- Change detection using timestamps
- Async scanning with UniTask
- Progress logging
- Image export with proper thread handling

#### ItemAcquisitionMonitor
Monitors inventory changes and records acquisitions.

```csharp
var monitor = new ItemAcquisitionMonitor(storageService);
monitor.StartMonitoringAsync(); // Subscribes to events

// Automatically records:
// - Character inventory changes
// - Player storage (warehouse) changes
// - Initial items on first run
```

**Features**:
- Real-time event monitoring
- Debouncing (300ms) to batch acquisitions
- Initialization with existing items
- Per-save-slot tracking

### Data Models

#### ItemMetadata
```csharp
public class ItemMetadata
{
    public int ItemId { get; set; }
    public string DisplayName { get; set; }
    public string DisplayNameKey { get; set; }
    public string DescriptionKey { get; set; }
    public bool Stackable { get; set; }
    public int MaxStackCount { get; set; }
    public float Weight { get; set; }
    public int Price { get; set; }
    public string DisplayQuality { get; set; }
    public List<string> Tags { get; set; }
    public List<StatEntry> Stats { get; set; }
    public int Order { get; set; }
    public DateTime LastModified { get; set; }
}
```

#### AcquisitionBatch
```csharp
public class AcquisitionBatch
{
    public DateTime Timestamp { get; set; }
    public List<int> Items { get; set; }
    public List<int> Quantities { get; set; }
}
```

## Configuration

Currently, all configuration is hardcoded:
- Debounce delay: 300ms
- Save slot: 0 (placeholder)
- Image format: PNG

Future versions may support configuration via config file.

## Troubleshooting

### No Items Exported
- Check game logs for errors
- Ensure ItemAssetsCollection.Instance is available
- Verify write permissions to temp directory

### Images Not Exporting
- Some textures are not readable (Unity engine limitation)
- Check log for "Images Skipped" count
- Non-readable textures are safely skipped

### History Not Recording
- Ensure player character is loaded
- Check that monitoring started after scene load
- Verify events are firing (check debug logs)

## Performance Considerations

- **Item Collection**: Runs on background thread, ~1000 items take 1-2 seconds
- **Image Export**: Main thread required for Unity API, batched with texture operations
- **History Recording**: Debounced to minimize I/O, typical batch size: 1-10 items
- **Change Detection**: Efficient timestamp comparison, only exports changed items

## License

This mod is part of the Newbe.Ducky project.
