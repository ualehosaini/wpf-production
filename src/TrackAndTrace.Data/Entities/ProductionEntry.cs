namespace TrackAndTrace.Data.Entities;

/// <summary>
/// EF Core entity that maps to the production entries table.
/// </summary>
public sealed class ProductionEntry
{
    /// <summary>
    /// Auto-incrementing primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Barcode or RFID value read for the item.
    /// </summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the station that processed the item.
    /// </summary>
    public string StationId { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp at which the item was processed.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// True when every plugin returned a passing result.
    /// </summary>
    public bool Passed { get; set; }

    /// <summary>
    /// Concatenated result messages from all plugins that ran on this item.
    /// </summary>
    public string PluginSummary { get; set; } = string.Empty;
}
