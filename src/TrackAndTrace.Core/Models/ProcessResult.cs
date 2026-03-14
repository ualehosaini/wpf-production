namespace TrackAndTrace.Core.Models;

/// <summary>
/// Encapsulates the outcome of a single plugin pass over a production item.
/// </summary>
public sealed class ProcessResult
{
    /// <summary>
    /// True when the plugin determined the item meets quality requirements.
    /// </summary>
    public bool Passed { get; init; }

    /// <summary>
    /// Name of the plugin that produced this result.
    /// </summary>
    public string PluginName { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable description of the outcome.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// UTC time at which the plugin completed processing.
    /// </summary>
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
}
