namespace SuperDucky.Connector.Models;

/// <summary>
/// Multi-language item descriptions for localization support.
/// Maps localization keys to translated text content.
/// </summary>
public class ItemDescription
{
    /// <summary>
    /// Schema version for data format compatibility.
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Map of language codes to description content.
    /// Key examples: "default", "en", "zh-CN", "ja", etc.
    /// </summary>
    [JsonProperty("languages")]
    public Dictionary<string, DescriptionContent> Languages { get; set; } = new();
}

/// <summary>
/// Description content for a specific language.
/// </summary>
public class DescriptionContent
{
    /// <summary>
    /// Localized item name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short description for UI tooltips.
    /// </summary>
    [JsonProperty("shortDescription")]
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Full/detailed description for item inspection.
    /// </summary>
    [JsonProperty("fullDescription")]
    public string FullDescription { get; set; } = string.Empty;
}
