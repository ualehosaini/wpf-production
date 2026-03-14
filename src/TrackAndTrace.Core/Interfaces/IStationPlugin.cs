using TrackAndTrace.Core.Models;

namespace TrackAndTrace.Core.Interfaces;

public interface IStationPlugin
{
    /// <summary>
    /// Display name of the plugin shown in the operator dashboard.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Current operational state of the plugin.
    /// </summary>
    PluginStatus Status { get; }

    /// <summary>
    /// Runs the plugin logic against the item and returns a pass or fail result.
    /// </summary>
    ProcessResult ProcessItem(ProductionItem item);

    /// <summary>
    /// Applies a runtime configuration update to the plugin.
    /// </summary>
    void Configure(PluginConfiguration config);
}
