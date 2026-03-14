namespace TrackAndTrace.Core.Models;

public sealed class PluginConfiguration
{
    public double FailureRate { get; set; } = 0.05;
    public IDictionary<string, string> Properties { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
