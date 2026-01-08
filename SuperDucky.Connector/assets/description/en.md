# SuperDucky.Connector

## Features

This mod automatically exports all game items and tracks your collection progress in real-time.

### Item Encyclopedia
Automatically exports comprehensive data for every item in the game, including:
- Item names and descriptions
- Item icons and preview images
- Detailed item properties (stackable, weight, price, quality, tags, stats)

### Acquisition Tracking
Tracks which items you've collected as you play:
- Real-time monitoring of your inventory
- Records item acquisition events with timestamps
- Supports multiple save slots with independent tracking

### How It Works
- Runs automatically when the game loads
- Saves data to your system's temp directory
- Updates continuously as you collect new items
- No manual configuration required

### Data Location
All exported data is saved in the system temp directory under `SuperDucky.Connector`:
- **Windows**: `C:\Users\{username}\AppData\Local\Temp\SuperDucky.Connector\`
- **Linux/macOS**: `/tmp/SuperDucky.Connector/`

External tools can read this data to display item information, analyze your collection progress, or integrate with other applications.
