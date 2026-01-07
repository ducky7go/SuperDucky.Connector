using SuperDucky.Connector.Models;

namespace SuperDucky.Connector;

/// <summary>
/// File I/O service for separated storage architecture.
/// Handles item folder creation and history file writing.
/// </summary>
public class FileStorageService : IDisposable
{
    private readonly string _basePath;
    private readonly string _itemsPath;
    private readonly string _historyPath;

    private readonly JsonSerializerSettings _jsonSettings;

    public FileStorageService()
    {
        // Use system temp path as base directory
        _basePath = Path.Combine(Application.persistentDataPath, ModOptions.FolderName, "SuperDucky.Connector", "Data");
        _itemsPath = Path.Combine(_basePath, "items");
        _historyPath = Path.Combine(_basePath, "history");
        _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatString = "yyyy-MM-ddTHH:mm:ssZ"
        };

        EnsureDirectoriesExist();
    }

    /// <summary>
    /// Ensures all required directory structures exist.
    /// </summary>
    private void EnsureDirectoriesExist()
    {
        try
        {
            // Create base directories
            CreateDirectoryIfNotExists(_basePath);
            CreateDirectoryIfNotExists(_itemsPath);
            CreateDirectoryIfNotExists(_historyPath);

            // Create digit folders (0-9) for item distribution
            for (var i = 0; i <= 9; i++)
            {
                CreateDirectoryIfNotExists(Path.Combine(_itemsPath, i.ToString()));
            }

            Log.Info($"[DuckyConnector] Directory structure ensured at: {_basePath}");
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Failed to create directory structure: {ex.Message}");
        }
    }

    private void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// Gets the folder digit for an item ID based on last digit.
    /// </summary>
    public static int GetFolderDigit(int itemId)
    {
        return Math.Abs(itemId) % 10;
    }

    /// <summary>
    /// Gets the item path for a specific item ID and save slot.
    /// Format: {temp}/SuperDucky.Connector/items/{save_slot}/{digit}/{itemId}/
    /// </summary>
    public string GetItemPath(int itemId, int saveSlot = 1)
    {
        var digit = GetFolderDigit(itemId);
        var saveSlotPath = Path.Combine(_itemsPath, saveSlot.ToString());
        var digitPath = Path.Combine(saveSlotPath, digit.ToString());
        var itemPath = Path.Combine(digitPath, itemId.ToString());

        // Ensure directories exist
        CreateDirectoryIfNotExists(saveSlotPath);
        CreateDirectoryIfNotExists(digitPath);
        CreateDirectoryIfNotExists(itemPath);

        return itemPath;
    }

    /// <summary>
    /// Writes item metadata to a JSON file.
    /// </summary>
    public async Task WriteItemMetadataAsync(int itemId, int saveSlot, ItemMetadata metadata)
    {
        try
        {
            var itemPath = GetItemPath(itemId, saveSlot);
            var metadataPath = Path.Combine(itemPath, "metadata.json");

            var json = JsonConvert.SerializeObject(metadata, _jsonSettings);
            await File.WriteAllTextAsync(metadataPath, json);
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Failed to write metadata for item {itemId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Writes item description to a JSON file.
    /// </summary>
    public async Task WriteItemDescriptionAsync(int itemId, int saveSlot, ItemDescription description)
    {
        try
        {
            var itemPath = GetItemPath(itemId, saveSlot);
            var descriptionPath = Path.Combine(itemPath, "description.json");

            var json = JsonConvert.SerializeObject(description, _jsonSettings);
            await File.WriteAllTextAsync(descriptionPath, json);
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Failed to write description for item {itemId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Exports a Unity Sprite to a PNG file.
    /// </summary>
    public async Task WriteItemImageAsync(int itemId, int saveSlot, Sprite sprite, bool isPreview = false)
    {
        try
        {
            if (sprite == null || sprite.texture == null)
            {
                return;
            }

            var itemPath = GetItemPath(itemId, saveSlot);
            var imagePath = Path.Combine(itemPath, isPreview ? "preview.png" : "icon.png");

            // Get the sprite texture
            var sourceTexture = sprite.texture;

            // Make texture readable if needed (must be on main thread)
            await UniTask.SwitchToMainThread();
            var readableTex = MakeTextureReadable(sourceTexture);

            // Calculate the sprite's region in the texture
            var textureRect = sprite.textureRect;
            var x = (int)textureRect.x;
            var y = (int)textureRect.y;
            var width = (int)textureRect.width;
            var height = (int)textureRect.height;

            // Get pixels from the sprite region
            var pixels = readableTex.GetPixels(x, y, width, height);

            // Create new texture with the same pixels
            var outputTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            outputTex.SetPixels(pixels);
            outputTex.Apply();

            // Encode to PNG
            var pngData = outputTex.EncodeToPNG();

            // Cleanup textures
            UnityEngine.Object.DestroyImmediate(readableTex);
            UnityEngine.Object.DestroyImmediate(outputTex);

            // Switch to background thread for file I/O
            await UniTask.SwitchToThreadPool();
            await File.WriteAllBytesAsync(imagePath, pngData);

            Log.Debug($"[DuckyConnector] Exported {imagePath} ({width}x{height})");
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Failed to write image for item {itemId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Converts texture to readable format, handling non-readable textures via RenderTexture.
    /// Must be called on main thread.
    /// </summary>
    private Texture2D MakeTextureReadable(Texture2D sourceTex)
    {
        // If texture is already readable, create a copy
        if (sourceTex.isReadable)
        {
            var copy = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);
            copy.SetPixels(sourceTex.GetPixels());
            copy.Apply();
            return copy;
        }

        // For non-readable textures, use RenderTexture blit
        // Use sRGB color space to match game's color rendering
        var renderTex = RenderTexture.GetTemporary(
            sourceTex.width,
            sourceTex.height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.sRGB
        );

        Graphics.Blit(sourceTex, renderTex);
        var previous = RenderTexture.active;
        RenderTexture.active = renderTex;

        var readableTex = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);
        readableTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableTex.Apply();

        // Restore state and cleanup
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        return readableTex;
    }

    /// <summary>
    /// Writes a time-sharded history batch file.
    /// Filename format: history_{yyyyMMdd_HHmmss}.json
    /// </summary>
    public async Task WriteHistoryBatchAsync(int saveSlot, AcquisitionBatch batch)
    {
        try
        {
            var saveSlotPath = Path.Combine(_historyPath, saveSlot.ToString());
            CreateDirectoryIfNotExists(saveSlotPath);

            var filename = $"history_{batch.Timestamp:yyyyMMdd_HHmmss}.json";
            var filepath = Path.Combine(saveSlotPath, filename);

            var json = JsonConvert.SerializeObject(batch, _jsonSettings);
            await File.WriteAllTextAsync(filepath, json);

            Log.Debug($"[DuckyConnector] Written history batch: {filename}");
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Failed to write history batch: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads existing item metadata if it exists.
    /// Returns null if the file doesn't exist.
    /// </summary>
    public async Task<ItemMetadata?> ReadItemMetadataAsync(int itemId, int saveSlot)
    {
        try
        {
            var itemPath = GetItemPath(itemId, saveSlot);
            var metadataPath = Path.Combine(itemPath, "metadata.json");

            if (!File.Exists(metadataPath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(metadataPath);
            return JsonConvert.DeserializeObject<ItemMetadata>(json);
        }
        catch (Exception ex)
        {
            Log.Error($"[DuckyConnector] Failed to read metadata for item {itemId}: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}