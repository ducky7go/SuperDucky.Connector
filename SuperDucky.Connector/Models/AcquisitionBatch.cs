namespace SuperDucky.Connector.Models;

/// <summary>
/// Single batch of item acquisitions for time-sharded history files.
/// Each batch represents items acquired together within a short time window.
/// </summary>
public class AcquisitionBatch
{
    /// <summary>
    /// Schema version for data format compatibility.
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Timestamp when this batch was recorded.
    /// </summary>
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Array of item IDs acquired together in this batch.
    /// </summary>
    [JsonProperty("items")]
    public List<int> Items { get; set; } = new();

    /// <summary>
    /// Array of quantities corresponding to each item in the Items array.
    /// quantities[i] is the quantity for items[i].
    /// </summary>
    [JsonProperty("quantities")]
    public List<int> Quantities { get; set; } = new();
}
