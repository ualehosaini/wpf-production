namespace TrackAndTrace.Core.Models;

public sealed class ProcessResult
{
    public bool Passed { get; init; }
    public string PluginName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
}
