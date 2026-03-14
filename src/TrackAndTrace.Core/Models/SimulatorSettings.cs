namespace TrackAndTrace.Core.Models;

/// <summary>
/// Holds configurable parameters that drive the production simulation.
/// </summary>
public sealed class SimulatorSettings
{
    /// <summary>
    /// Number of production items processed per minute.
    /// </summary>
    public int ItemsPerMinute { get; set; } = 30;

    /// <summary>
    /// Fraction of items that deliberately fail, expressed as a value between 0 and 1.
    /// </summary>
    public double FailureRate { get; set; } = 0.05;

    /// <summary>
    /// Identifier for the production station used in all emitted items.
    /// </summary>
    public string StationId { get; set; } = "STATION-01";
}
