namespace TrackAndTrace.Data.Entities;

public sealed class ProductionEntry
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string StationId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool Passed { get; set; }
    public string PluginSummary { get; set; } = string.Empty;
}
