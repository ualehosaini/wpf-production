namespace TrackAndTrace.Core.Models;

public sealed class ProductionItem
{
    public string SerialNumber { get; set; } = string.Empty;
    public string StationId { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
