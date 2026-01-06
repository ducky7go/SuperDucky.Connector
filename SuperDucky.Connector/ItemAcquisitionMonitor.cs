using ItemStatsSystem;
using SuperDucky.Connector.Models;

namespace SuperDucky.Connector;

/// <summary>
/// Monitors item acquisition events through inventory change subscriptions.
/// Implements batch recording and debouncing for efficient I/O.
/// </summary>
public class ItemAcquisitionMonitor : IDisposable
{
    private readonly FileStorageService _storageService;

    // Debouncing state
    private readonly List<PendingAcquisition> _pendingAcquisitions = new();
    private readonly object _pendingLock = new();
    private bool _debounceTimerActive;
    private const int DebounceMilliseconds = 300;

    // Tracking state
    private readonly Dictionary<int, HashSet<int>> _collectedItemsBySaveSlot = new();
    private readonly Dictionary<int, HashSet<int>> _previousInventorySnapshot = new();

    // Initialization state
    private bool _isInitialized;

    public ItemAcquisitionMonitor(FileStorageService storageService)
    {
        _storageService = storageService;
    }

    /// <summary>
    /// Starts monitoring inventory changes for item acquisition.
    /// </summary>
    public async void StartMonitoringAsync()
    {
        try
        {
            // Wait for player to be available
            await UniTask.WaitUntil(() => LevelManager.Instance?.MainCharacter != null);

            var mainCharacter = LevelManager.Instance?.MainCharacter;
            if (mainCharacter?.CharacterItem == null)
            {
                Log.Warn("[DuckyConnector] Main character not available, deferring monitoring start");
                return;
            }

            // Subscribe to main character inventory changes
            mainCharacter.CharacterItem.Inventory.onContentChanged += OnMainCharacterInventoryChanged;

            // Subscribe to player storage changes
            PlayerStorage.OnPlayerStorageChange += OnPlayerStorageChanged;

            Log.Info("[DuckyConnector] Item acquisition monitoring started");

            // Initialize with existing items
            await InitializeWithExistingItemsAsync();
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Error starting acquisition monitoring: {ex.Message}");
        }
    }

    /// <summary>
    /// Initializes the monitor with all existing items in inventory and storage.
    /// Creates initial acquisition records for items the player already has.
    /// </summary>
    private async Task InitializeWithExistingItemsAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        try
        {
            int saveSlot = GetCurrentSaveSlot();
            var existingItems = new List<ItemAcquisitionData>();

            // Collect items from main character inventory
            var mainCharacter = LevelManager.Instance?.MainCharacter;
            int characterItemCount = 0;
            if (mainCharacter?.CharacterItem?.Inventory != null)
            {
                foreach (var item in mainCharacter.CharacterItem.Inventory)
                {
                    if (item != null)
                    {
                        characterItemCount++;
                        existingItems.Add(new ItemAcquisitionData
                        {
                            ItemId = item.TypeID,
                            ItemName = item.DisplayName ?? string.Empty,
                            Quantity = item.StackCount,
                            Source = "CharacterInventory"
                        });

                        // Track as already collected
                        if (!_collectedItemsBySaveSlot.ContainsKey(saveSlot))
                        {
                            _collectedItemsBySaveSlot[saveSlot] = new HashSet<int>();
                        }
                        _collectedItemsBySaveSlot[saveSlot].Add(item.TypeID);
                    }
                }
            }

            // Collect items from player storage
            int storageItemCount = 0;
            if (PlayerStorage.Inventory != null)
            {
                foreach (var item in PlayerStorage.Inventory)
                {
                    if (item != null)
                    {
                        storageItemCount++;
                        existingItems.Add(new ItemAcquisitionData
                        {
                            ItemId = item.TypeID,
                            ItemName = item.DisplayName ?? string.Empty,
                            Quantity = item.StackCount,
                            Source = "PlayerStorage"
                        });

                        // Track as already collected
                        if (!_collectedItemsBySaveSlot.ContainsKey(saveSlot))
                        {
                            _collectedItemsBySaveSlot[saveSlot] = new HashSet<int>();
                        }
                        _collectedItemsBySaveSlot[saveSlot].Add(item.TypeID);
                    }
                }
            }

            // Create initial snapshot for change detection
            _previousInventorySnapshot[saveSlot] = new HashSet<int>(
                existingItems.Select(x => x.ItemId)
            );

            // Write initial acquisition batch if there are items
            if (existingItems.Count > 0)
            {
                // Get unique item count
                var uniqueItemIds = new HashSet<int>(existingItems.Select(x => x.ItemId));

                var batch = new AcquisitionBatch
                {
                    Timestamp = DateTime.UtcNow,
                    Items = existingItems.Select(x => x.ItemId).ToList(),
                    Quantities = existingItems.Select(x => x.Quantity).ToList()
                };

                await _storageService.WriteHistoryBatchAsync(saveSlot, batch);

                Log.Info($"[DuckyConnector] Initialized with {existingItems.Count} total items ({uniqueItemIds.Count} unique)");
                Log.Info($"[DuckyConnector]   - Character inventory: {characterItemCount} items");
                Log.Info($"[DuckyConnector]   - Player storage: {storageItemCount} items");
            }
            else
            {
                Log.Info("[DuckyConnector] Initialized with no existing items");
            }

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Error initializing with existing items: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles main character inventory content changes.
    /// </summary>
    private void OnMainCharacterInventoryChanged(Inventory inventory, int index)
    {
        try
        {
            var item = inventory.GetItemAt(index);
            if (item != null)
            {
                // Item added or modified
                Log.Debug($"[DuckyConnector] Inventory change at index {index}: {item.DisplayName} (x{item.StackCount})");
                RecordItemAcquisition(item, item.StackCount, "CharacterInventory");
            }
            else
            {
                // Item removed - just log it, don't record as acquisition
                Log.Debug($"[DuckyConnector] Item removed from inventory at index {index}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Error handling inventory change: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles player storage content changes.
    /// </summary>
    private void OnPlayerStorageChanged(PlayerStorage storage, Inventory inventory, int index)
    {
        try
        {
            var item = inventory.GetItemAt(index);
            if (item != null)
            {
                // Item added or modified
                Log.Debug($"[DuckyConnector] Storage change at index {index}: {item.DisplayName} (x{item.StackCount})");
                RecordItemAcquisition(item, item.StackCount, "PlayerStorage");
            }
            else
            {
                // Item removed - just log it, don't record as acquisition
                Log.Debug($"[DuckyConnector] Item removed from storage at index {index}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Error handling storage change: {ex.Message}");
        }
    }

    /// <summary>
    /// Records an item acquisition event.
    /// Called when an item is added to the player's inventory or storage.
    /// </summary>
    private void RecordItemAcquisition(Item item, int quantity, string source)
    {
        if (item == null)
        {
            return;
        }

        int itemId = item.TypeID;
        int saveSlot = GetCurrentSaveSlot();

        lock (_pendingLock)
        {
            // Check if this item was already collected
            if (!_collectedItemsBySaveSlot.ContainsKey(saveSlot))
            {
                _collectedItemsBySaveSlot[saveSlot] = new HashSet<int>();
            }

            bool isNewItem = !_collectedItemsBySaveSlot[saveSlot].Contains(itemId);

            // Add to pending acquisitions
            _pendingAcquisitions.Add(new PendingAcquisition
            {
                ItemId = itemId,
                Quantity = quantity,
                ItemName = item.DisplayName ?? string.Empty,
                Source = source,
                Timestamp = DateTime.UtcNow,
                IsNewItem = isNewItem
            });

            // Start debounce timer if not already active
            if (!_debounceTimerActive)
            {
                _debounceTimerActive = true;
                Task.Delay(DebounceMilliseconds).ContinueWith(_ => FlushPendingAcquisitions());
            }
        }
    }

    /// <summary>
    /// Stops monitoring inventory changes.
    /// </summary>
    public void StopMonitoring()
    {
        try
        {
            // Unsubscribe from events
            var mainCharacter = LevelManager.Instance?.MainCharacter;
            if (mainCharacter?.CharacterItem?.Inventory != null)
            {
                mainCharacter.CharacterItem.Inventory.onContentChanged -= OnMainCharacterInventoryChanged;
            }

            PlayerStorage.OnPlayerStorageChange -= OnPlayerStorageChanged;

            // Flush any pending acquisitions
            FlushPendingAcquisitions().Wait();

            Log.Info("[DuckyConnector] Item acquisition monitoring stopped");
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Error stopping acquisition monitoring: {ex.Message}");
        }
    }

    /// <summary>
    /// Flushes all pending acquisitions to history file(s).
    /// Groups multiple items acquired together into batch entries.
    /// </summary>
    private async Task FlushPendingAcquisitions()
    {
        List<PendingAcquisition> toFlush;

        lock (_pendingLock)
        {
            if (_pendingAcquisitions.Count == 0)
            {
                _debounceTimerActive = false;
                return;
            }

            toFlush = new List<PendingAcquisition>(_pendingAcquisitions);
            _pendingAcquisitions.Clear();
            _debounceTimerActive = false;
        }

        try
        {
            int saveSlot = GetCurrentSaveSlot();

            // Group items by type ID (stack same items together)
            var groupedAcquisitions = toFlush
                .GroupBy(a => a.ItemId)
                .Select(g => new GroupedAcquisition
                {
                    ItemId = g.Key,
                    Quantity = g.Sum(a => a.Quantity),
                    ItemName = g.First().ItemName,
                    Source = g.First().Source,
                    IsNew = g.Any(a => a.IsNewItem)
                })
                .ToList();

            // Create acquisition batch
            var batch = new AcquisitionBatch
            {
                Timestamp = DateTime.UtcNow,
                Items = groupedAcquisitions.Select(a => a.ItemId).ToList(),
                Quantities = groupedAcquisitions.Select(a => a.Quantity).ToList()
            };

            // Write history batch
            await _storageService.WriteHistoryBatchAsync(saveSlot, batch);

            // Update collected items tracking
            if (!_collectedItemsBySaveSlot.ContainsKey(saveSlot))
            {
                _collectedItemsBySaveSlot[saveSlot] = new HashSet<int>();
            }

            foreach (var acquisition in groupedAcquisitions)
            {
                _collectedItemsBySaveSlot[saveSlot].Add(acquisition.ItemId);
            }

            var sources = string.Join(", ", groupedAcquisitions.Select(a => a.Source).Distinct());
            Log.Debug($"[DuckyConnector] Flushed {toFlush.Count} acquisitions as {groupedAcquisitions.Count} unique items from [{sources}]");
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Error flushing acquisitions: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current save slot from the game's SavesSystem.
    /// </summary>
    private int GetCurrentSaveSlot()
    {
        try
        {
            return Saves.SavesSystem.CurrentSlot;
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Failed to get current save slot: {ex.Message}");
            return 1; // Default to slot 1 if there's an error
        }
    }

    /// <summary>
    /// Checks if an item has been collected.
    /// </summary>
    public bool IsItemCollected(int itemId, int saveSlot)
    {
        return _collectedItemsBySaveSlot.ContainsKey(saveSlot) &&
               _collectedItemsBySaveSlot[saveSlot].Contains(itemId);
    }

    public void Dispose()
    {
        StopMonitoring();
        _collectedItemsBySaveSlot.Clear();
        _previousInventorySnapshot.Clear();
    }

    /// <summary>
    /// Represents a pending acquisition waiting to be flushed.
    /// </summary>
    private class PendingAcquisition
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsNewItem { get; set; }
    }

    /// <summary>
    /// Represents a grouped acquisition (same item type acquired together).
    /// </summary>
    private class GroupedAcquisition
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public bool IsNew { get; set; }
    }

    /// <summary>
    /// Represents item acquisition data for initialization.
    /// </summary>
    private class ItemAcquisitionData
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
