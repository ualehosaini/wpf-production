namespace TrackAndTrace.Core.Models;

/// <summary>
/// Represents an item moving through the production line before it is finalised.
/// </summary>
public sealed class ProductionItem
{
    /// <summary>
    /// Barcode or RFID value assigned by the scanner plugin.
    /// </summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the station processing this item.
    /// </summary>
    public string StationId { get; init; } = string.Empty;

    /// <summary>
    /// UTC time at which the item entered the pipeline.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
