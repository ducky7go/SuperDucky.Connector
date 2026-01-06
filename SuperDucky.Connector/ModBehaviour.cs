using Duckov.Utilities;

namespace SuperDucky.Connector;

/// <summary>
/// Main mod entry point for Super Ducky Connector.
/// Initializes the mod, sets up Harmony patching, and coordinates all components.
/// </summary>
public class ModBehaviour : ModBehaviourBase
{
    private ItemDictionaryTracker? _itemDictionaryTracker;
    private ItemAcquisitionMonitor? _itemAcquisitionMonitor;
    private FileStorageService? _fileStorageService;

    protected override void ModEnabled()
    {
        Log.Info("[DuckyConnector] Super Ducky Connector mod is loading...");

        // Initialize services
        _fileStorageService = new FileStorageService();
        _itemDictionaryTracker = new ItemDictionaryTracker(_fileStorageService);
        _itemAcquisitionMonitor = new ItemAcquisitionMonitor(_fileStorageService);

        // Subscribe to scene change events
        SceneLoader.onAfterSceneInitialize += OnAfterSceneInitialize;

        Log.Info("[DuckyConnector] Super Ducky Connector mod loaded successfully");
    }

    protected override void ModDisabled()
    {
        // Unsubscribe from scene change events
        SceneLoader.onAfterSceneInitialize -= OnAfterSceneInitialize;

        // Clean up resources
        _itemAcquisitionMonitor?.Dispose();
        _itemDictionaryTracker?.Dispose();
        _fileStorageService?.Dispose();

        // Unpatch all Harmony patches

        Log.Info("[DuckyConnector] Super Ducky Connector mod unloaded");
    }

    private void OnAfterSceneInitialize(SceneLoadingContext context)
    {
        // Trigger data collection when loading into the main game scene
        if (context.sceneName == GameplayDataSettings.SceneManagement.BaseScene.Name)
        {
            Log.Info("[DuckyConnector] Main scene loaded, starting data collection...");

            // Start item encyclopedia collection
            _itemDictionaryTracker?.StartCollectionAsync();

            // Start item acquisition monitoring
            _itemAcquisitionMonitor?.StartMonitoringAsync();
        }
    }
}