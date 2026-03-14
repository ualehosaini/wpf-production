namespace TrackAndTrace.Core.Models;

/// <summary>
/// Immutable record representing a completed inspection event stored in the database.
/// </summary>
public sealed class ProductionRecord
{
    /// <summary>
    /// Primary key assigned by the database.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Barcode or RFID value read for this item.
    /// </summary>
    public string SerialNumber { get; init; } = string.Empty;

    /// <summary>
    /// Identifier of the station that processed this item.
    /// </summary>
    public string StationId { get; init; } = string.Empty;

    /// <summary>
    /// UTC time at which the item was processed.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// True when all plugins returned a passing result.
    /// </summary>
    public bool Passed { get; init; }

    /// <summary>
    /// Concatenated messages from every plugin that ran against this item.
    /// </summary>
    public string PluginSummary { get; init; } = string.Empty;
}
