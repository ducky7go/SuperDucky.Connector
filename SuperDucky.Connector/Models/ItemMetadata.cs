namespace SuperDucky.Connector.Models;

/// <summary>
/// Core item properties exported from ItemStatsSystem.Item class.
/// Contains all item metadata including timestamps for change detection.
/// </summary>
public class ItemMetadata
{
    /// <summary>
    /// Schema version for data format compatibility.
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Item type identifier (from Item.TypeID).
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// Duplicate of Id for clarity (from Item.TypeID).
    /// </summary>
    [JsonProperty("typeID")]
    public int TypeID { get; set; }

    /// <summary>
    /// Localization key for the item name (from Item.DisplayNameRaw).
    /// </summary>
    [JsonProperty("displayNameKey")]
    public string DisplayNameKey { get; set; } = string.Empty;

    /// <summary>
    /// Raw display name string (from Item.DisplayNameRaw).
    /// </summary>
    [JsonProperty("displayNameRaw")]
    public string DisplayNameRaw { get; set; } = string.Empty;

    /// <summary>
    /// Localization key for the item description (from Item.DescriptionRaw).
    /// </summary>
    [JsonProperty("descriptionKey")]
    public string DescriptionKey { get; set; } = string.Empty;

    /// <summary>
    /// Display order in the encyclopedia (from Item.Order).
    /// </summary>
    [JsonProperty("order")]
    public int Order { get; set; }

    /// <summary>
    /// Maximum number that can be stacked (from Item.MaxStackCount).
    /// </summary>
    [JsonProperty("maxStackCount")]
    public int MaxStackCount { get; set; }

    /// <summary>
    /// Whether the item can be stacked (from Item.Stackable).
    /// </summary>
    [JsonProperty("stackable")]
    public bool Stackable { get; set; }

    /// <summary>
    /// Base value/price of the item (from Item.Value).
    /// </summary>
    [JsonProperty("value")]
    public int Value { get; set; }

    /// <summary>
    /// Quality tier number (from Item.Quality).
    /// </summary>
    [JsonProperty("quality")]
    public int Quality { get; set; }

    /// <summary>
    /// Quality display name (from Item.DisplayQuality).
    /// </summary>
    [JsonProperty("displayQuality")]
    public string DisplayQuality { get; set; } = string.Empty;

    /// <summary>
    /// Weight per unit (from Item.UnitSelfWeight).
    /// </summary>
    [JsonProperty("weight")]
    public float Weight { get; set; }

    /// <summary>
    /// Item tags for categorization (from Item.Tags).
    /// </summary>
    [JsonProperty("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Item statistics (from Item.Stats).
    /// </summary>
    [JsonProperty("stats")]
    public Dictionary<string, float> Stats { get; set; } = new();

    /// <summary>
    /// Available slots on the item (from Item.Slots).
    /// </summary>
    [JsonProperty("slots")]
    public List<object> Slots { get; set; } = new();

    /// <summary>
    /// Item modifiers/bonuses (from Item.Modifiers).
    /// </summary>
    [JsonProperty("modifiers")]
    public List<object> Modifiers { get; set; } = new();

    /// <summary>
    /// Whether the item uses durability system (from Item.UseDurability).
    /// </summary>
    [JsonProperty("useDurability")]
    public bool UseDurability { get; set; }

    /// <summary>
    /// Maximum durability value (from Item.MaxDurability).
    /// </summary>
    [JsonProperty("maxDurability")]
    public float MaxDurability { get; set; }

    /// <summary>
    /// Time required to use the item (from Item.UseTime).
    /// </summary>
    [JsonProperty("useTime")]
    public float UseTime { get; set; }

    /// <summary>
    /// Whether the item can be sold to vendors (from Item.CanBeSold).
    /// </summary>
    [JsonProperty("canBeSold")]
    public bool CanBeSold { get; set; }

    /// <summary>
    /// Whether the item can be dropped by the player (from Item.CanDrop).
    /// </summary>
    [JsonProperty("canDrop")]
    public bool CanDrop { get; set; }

    /// <summary>
    /// Sound effect key for the item (from Item.SoundKey).
    /// </summary>
    [JsonProperty("soundKey")]
    public string SoundKey { get; set; } = "default";

    /// <summary>
    /// Timestamp when the item was first discovered/collected.
    /// </summary>
    [JsonProperty("firstSeenAt")]
    public DateTime FirstSeenAt { get; set; }

    /// <summary>
    /// Timestamp when the item data was last updated (change detection).
    /// </summary>
    [JsonProperty("lastUpdatedAt")]
    public DateTime LastUpdatedAt { get; set; }
}
