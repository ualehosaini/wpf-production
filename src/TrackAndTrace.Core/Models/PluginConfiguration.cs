namespace TrackAndTrace.Core.Models;

/// <summary>
/// Carries runtime settings that can be pushed to any station plugin.
/// </summary>
public sealed class PluginConfiguration
{
    /// <summary>
    /// Override failure rate applied when the plugin supports simulation mode.
    /// </summary>
    public double FailureRate { get; set; } = 0.05;

    /// <summary>
    /// Arbitrary key-value pairs forwarded to the plugin for custom configuration.
    /// </summary>
    public IDictionary<string, string> Properties { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
