namespace TrackAndTrace.Core.Models;

public sealed class ProductionRecord
{
    public int Id { get; init; }
    public string SerialNumber { get; init; } = string.Empty;
    public string StationId { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public bool Passed { get; init; }
    public string PluginSummary { get; init; } = string.Empty;
}
