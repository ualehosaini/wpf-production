using TrackAndTrace.Core.Interfaces;
using TrackAndTrace.Core.Models;

namespace TrackAndTrace.Host.Services;

/// <summary>
/// Coordinates the execution of all registered station plugins for each production item.
/// </summary>
public sealed class PluginOrchestrator
{
    private readonly IReadOnlyList<IStationPlugin> _plugins;

    /// <summary>
    /// Stores the ordered list of plugins to run for each production item.
    /// </summary>
    public PluginOrchestrator(IEnumerable<IStationPlugin> plugins)
    {
        _plugins = plugins.ToList();
    }

    /// <summary>
    /// Ordered list of plugins executed for every item that enters the pipeline.
    /// </summary>
    public IReadOnlyList<IStationPlugin> Plugins => _plugins;

    /// <summary>
    /// Runs every registered plugin against the item and returns the collected results.
    /// </summary>
    public IReadOnlyList<ProcessResult> Process(ProductionItem item)
    {
        return _plugins
            .Select(p => p.ProcessItem(item))
            .ToList();
    }

    /// <summary>
    /// Forwards a configuration update to every registered plugin.
    /// </summary>
    public void ApplyConfiguration(PluginConfiguration config)
    {
        foreach (var plugin in _plugins)
            plugin.Configure(config);
    }
}
