using ItemStatsSystem;
using SuperDucky.Connector.Models;

namespace SuperDucky.Connector;

/// <summary>
/// Tracks and exports item encyclopedia data from ItemStatsSystem.Item.
/// Implements change detection to only export changed items.
/// </summary>
public class ItemDictionaryTracker : IDisposable
{
    private readonly FileStorageService _storageService;
    private readonly Dictionary<int, HashSet<string>> _collectedItems; // saveSlot -> itemIDs

    public ItemDictionaryTracker(FileStorageService storageService)
    {
        _storageService = storageService;
        _collectedItems = new Dictionary<int, HashSet<string>>();
    }

    /// <summary>
    /// Starts asynchronous item collection from the game's item encyclopedia.
    /// Runs on background thread to avoid blocking main thread.
    /// </summary>
    public void StartCollectionAsync()
    {
        // Run on background thread to avoid blocking main thread
        StartCollectionInBackground().Forget();
    }

    /// <summary>
    /// Runs item collection on a background thread using UniTask.
    /// </summary>
    private async UniTaskVoid StartCollectionInBackground()
    {
        try
        {
            // Switch to ThreadPool to run off the main thread
            await UniTask.SwitchToThreadPool();

            int currentSaveSlot = GetCurrentSaveSlot();
            Log.Info($"[DuckyConnector] Starting item collection for save slot {currentSaveSlot}...");

            if (!_collectedItems.ContainsKey(currentSaveSlot))
            {
                _collectedItems[currentSaveSlot] = new HashSet<string>();
            }

            await CollectAllItemsAsync(currentSaveSlot);

            Log.Info($"[DuckyConnector] Item collection complete for save slot {currentSaveSlot}");
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Error during item collection: {ex.Message}");
        }
    }

    /// <summary>
    /// Collects all items from ItemAssetsCollection and exports them.
    /// </summary>
    private async Task CollectAllItemsAsync(int saveSlot)
    {
        var collection = ItemAssetsCollection.Instance;
        if (collection == null)
        {
            Log.Error("[DuckyConnector] ItemAssetsCollection.Instance is null");
            return;
        }

        int totalCount = collection.entries.Count;
        int exportCount = 0;
        int skipCount = 0;
        int errorCount = 0;
        int imageExportCount = 0;
        int imageSkipCount = 0;

        Log.Info($"[DuckyConnector] Found {totalCount} items in ItemAssetsCollection");

        foreach (var entry in collection.entries)
        {
            if (entry.prefab == null)
            {
                continue;
            }

            Item item = entry.prefab;
            int itemId = item.TypeID;

            try
            {
                bool exported = await ExportItemIfChangedAsync(item, saveSlot);
                if (exported)
                {
                    exportCount++;
                    // Count successful image exports
                    if (item.Icon != null && item.Icon.texture != null && item.Icon.texture.isReadable)
                    {
                        imageExportCount++;
                    }
                    else
                    {
                        imageSkipCount++;
                    }
                }
                else
                {
                    skipCount++;
                }

                _collectedItems[saveSlot].Add(itemId.ToString());
            }
            catch (Exception ex)
            {
                errorCount++;
                Log.Error($"[DuckyConnector] Error exporting item {itemId}: {ex.Message}");
            }
        }

        // Log comprehensive statistics
        Log.Info("========================================");
        Log.Info("[DuckyConnector] Item Collection Summary");
        Log.Info("========================================");
        Log.Info($"Total Items Found:        {totalCount}");
        Log.Info($"Items Exported:           {exportCount}");
        Log.Info($"Items Skipped (unchanged): {skipCount}");
        Log.Info($"Errors:                   {errorCount}");
        Log.Info($"Images Exported:          {imageExportCount}");
        Log.Info($"Images Skipped:           {imageSkipCount} (unreadable or null)");
        Log.Info($"Export Location:          {Path.GetTempPath()}SuperDucky.Connector/");
        Log.Info("========================================");
    }

    /// <summary>
    /// Exports an item if its data has changed since last export.
    /// Implements change detection for efficient updates.
    /// </summary>
    private async Task<bool> ExportItemIfChangedAsync(Item item, int saveSlot)
    {
        int itemId = item.TypeID;
        ItemMetadata? existingMetadata = await _storageService.ReadItemMetadataAsync(itemId, saveSlot);

        // Create new metadata from current item state
        ItemMetadata newMetadata = CreateItemMetadata(item, existingMetadata);

        // Check if item has changed
        if (existingMetadata != null && !HasItemChanged(item, existingMetadata))
        {
            return false; // Item unchanged, skip export
        }

        // Export item data
        await ExportItemAsync(item, saveSlot, newMetadata);
        return true;
    }

    /// <summary>
    /// Creates ItemMetadata from an Item instance.
    /// Preserves FirstSeenAt timestamp for existing items.
    /// </summary>
    private ItemMetadata CreateItemMetadata(Item item, ItemMetadata? existingMetadata)
    {
        DateTime now = DateTime.UtcNow;
        DateTime firstSeenAt = existingMetadata?.FirstSeenAt ?? now;

        // Collect stats
        var stats = new Dictionary<string, float>();
        if (item.Stats != null)
        {
            foreach (var stat in item.Stats)
            {
                if (stat != null && !string.IsNullOrEmpty(stat.Key))
                {
                    stats[stat.Key] = stat.Value;
                }
            }
        }

        // Collect tags
        var tags = new List<string>();
        if (item.Tags != null)
        {
            foreach (var tag in item.Tags)
            {
                if (tag != null && !string.IsNullOrEmpty(tag.DisplayName))
                {
                    tags.Add(tag.DisplayName);
                }
            }
        }

        return new ItemMetadata
        {
            Id = item.TypeID,
            TypeID = item.TypeID,
            DisplayNameKey = item.DisplayNameRaw ?? string.Empty,
            DisplayNameRaw = item.DisplayName ?? string.Empty,
            DescriptionKey = item.DescriptionRaw ?? string.Empty,
            Order = item.Order,
            MaxStackCount = item.MaxStackCount,
            Stackable = item.Stackable,
            Value = item.Value,
            Quality = item.Quality,
            DisplayQuality = item.DisplayQuality.ToString(),
            Weight = item.UnitSelfWeight,
            Tags = tags,
            Stats = stats,
            Slots = new List<object>(), // Simplified for initial implementation
            Modifiers = new List<object>(), // Simplified for initial implementation
            UseDurability = item.UseDurability,
            MaxDurability = item.MaxDurability,
            UseTime = item.UseTime,
            CanBeSold = item.CanBeSold,
            CanDrop = item.CanDrop,
            SoundKey = item.SoundKey ?? "default",
            FirstSeenAt = firstSeenAt,
            LastUpdatedAt = now
        };
    }

    /// <summary>
    /// Detects if item data has changed compared to previously exported metadata.
    /// Primary change indicator: DisplayNameRaw
    /// Secondary indicators: Value, Quality, Tags, Stats, etc.
    /// </summary>
    private bool HasItemChanged(Item item, ItemMetadata existing)
    {
        // Primary: display name change (most important)
        if (item.DisplayNameRaw != existing.DisplayNameRaw)
        {
            Log.Debug($"[DuckyConnector] Item {item.TypeID} changed: DisplayNameRaw");
            return true;
        }

        // Secondary: other property changes
        if (item.Value != existing.Value)
        {
            Log.Debug($"[DuckyConnector] Item {item.TypeID} changed: Value");
            return true;
        }

        if (item.Quality != existing.Quality)
        {
            Log.Debug($"[DuckyConnector] Item {item.TypeID} changed: Quality");
            return true;
        }

        if (item.MaxStackCount != existing.MaxStackCount)
        {
            Log.Debug($"[DuckyConnector] Item {item.TypeID} changed: MaxStackCount");
            return true;
        }

        if (Math.Abs(item.UnitSelfWeight - existing.Weight) > 0.01f)
        {
            Log.Debug($"[DuckyConnector] Item {item.TypeID} changed: Weight");
            return true;
        }

        // Tags comparison
        var currentTags = new HashSet<string>(
            item.Tags?.Select(t => t?.name).Where(n => !string.IsNullOrEmpty(n)) ?? Enumerable.Empty<string>()
        );

        var existingTags = new HashSet<string>(existing.Tags ?? Enumerable.Empty<string>());

        if (!currentTags.SetEquals(existingTags))
        {
            Log.Debug($"[DuckyConnector] Item {item.TypeID} changed: Tags");
            return true;
        }

        // Stats comparison (compare stat values)
        if (item.Stats != null)
        {
            foreach (var stat in item.Stats)
            {
                if (stat != null && !string.IsNullOrEmpty(stat.Key))
                {
                    bool statExists = existing.Stats.TryGetValue(stat.Key, out float existingValue);
                    if (!statExists || Math.Abs(stat.Value - existingValue) > 0.01f)
                    {
                        Log.Debug($"[DuckyConnector] Item {item.TypeID} changed: Stats[{stat.Key}]");
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Exports all item data (metadata, description, images).
    /// </summary>
    private async Task ExportItemAsync(Item item, int saveSlot, ItemMetadata metadata)
    {
        int itemId = item.TypeID;

        // Write metadata
        await _storageService.WriteItemMetadataAsync(itemId, saveSlot, metadata);

        // Write description (multi-language)
        ItemDescription description = CreateItemDescription(item);
        await _storageService.WriteItemDescriptionAsync(itemId, saveSlot, description);

        // Write images
        await _storageService.WriteItemImageAsync(itemId, saveSlot, item.Icon, isPreview: false);
        // Note: Preview image not currently available in Item class
    }

    /// <summary>
    /// Creates ItemDescription with multi-language support.
    /// </summary>
    private ItemDescription CreateItemDescription(Item item)
    {
        var description = new ItemDescription();
        var defaultContent = new DescriptionContent
        {
            Name = item.DisplayName ?? string.Empty,
            ShortDescription = string.Empty, // Not directly available
            FullDescription = item.Description ?? string.Empty
        };

        description.Languages["default"] = defaultContent;

        // Note: Full multi-language support would require accessing SodaCraft.Localizations
        // For now, we include the default language which is typically what the game is using

        return description;
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
        return _collectedItems.ContainsKey(saveSlot) &&
               _collectedItems[saveSlot].Contains(itemId.ToString());
    }

    public void Dispose()
    {
        _collectedItems.Clear();
    }
}
