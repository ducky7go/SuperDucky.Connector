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
            for (int i = 0; i <= 9; i++)
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
        int digit = GetFolderDigit(itemId);
        string saveSlotPath = Path.Combine(_itemsPath, saveSlot.ToString());
        string digitPath = Path.Combine(saveSlotPath, digit.ToString());
        string itemPath = Path.Combine(digitPath, itemId.ToString());

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
            string itemPath = GetItemPath(itemId, saveSlot);
            string metadataPath = Path.Combine(itemPath, "metadata.json");

            string json = JsonConvert.SerializeObject(metadata, _jsonSettings);
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
            string itemPath = GetItemPath(itemId, saveSlot);
            string descriptionPath = Path.Combine(itemPath, "description.json");

            string json = JsonConvert.SerializeObject(description, _jsonSettings);
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

            string itemPath = GetItemPath(itemId, saveSlot);
            string imagePath = Path.Combine(itemPath, isPreview ? "preview.png" : "icon.png");

            // Get the sprite texture
            Texture2D sourceTexture = sprite.texture;

            // Make texture readable if needed (must be on main thread)
            await UniTask.SwitchToMainThread();
            Texture2D readableTex = MakeTextureReadable(sourceTexture);

            // Calculate the sprite's region in the texture
            Rect textureRect = sprite.textureRect;
            int x = (int)textureRect.x;
            int y = (int)textureRect.y;
            int width = (int)textureRect.width;
            int height = (int)textureRect.height;

            // Get pixels from the sprite region
            Color[] pixels = readableTex.GetPixels(x, y, width, height);

            // Create new texture with the same pixels
            Texture2D outputTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            outputTex.SetPixels(pixels);
            outputTex.Apply();

            // Encode to PNG
            byte[] pngData = outputTex.EncodeToPNG();

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
            Texture2D copy = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);
            copy.SetPixels(sourceTex.GetPixels());
            copy.Apply();
            return copy;
        }

        // For non-readable textures, use RenderTexture blit
        // Use sRGB color space to match game's color rendering
        RenderTexture renderTex = RenderTexture.GetTemporary(
            sourceTex.width,
            sourceTex.height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.sRGB
        );

        Graphics.Blit(sourceTex, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;

        Texture2D readableTex = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);
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
            string saveSlotPath = Path.Combine(_historyPath, saveSlot.ToString());
            CreateDirectoryIfNotExists(saveSlotPath);

            string filename = $"history_{batch.Timestamp:yyyyMMdd_HHmmss}.json";
            string filepath = Path.Combine(saveSlotPath, filename);

            string json = JsonConvert.SerializeObject(batch, _jsonSettings);
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
            string itemPath = GetItemPath(itemId, saveSlot);
            string metadataPath = Path.Combine(itemPath, "metadata.json");

            if (!File.Exists(metadataPath))
            {
                return null;
            }

            string json = await File.ReadAllTextAsync(metadataPath);
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