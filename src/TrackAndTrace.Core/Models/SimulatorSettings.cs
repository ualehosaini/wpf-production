namespace TrackAndTrace.Core.Models;

public sealed class SimulatorSettings
{
    public int ItemsPerMinute { get; set; } = 30;
    public double FailureRate { get; set; } = 0.05;
    public string StationId { get; set; } = "STATION-01";
}
