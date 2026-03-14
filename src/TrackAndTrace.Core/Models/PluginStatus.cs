namespace TrackAndTrace.Core.Models;

/// <summary>
/// Represents the current operational state of a station plugin.
/// </summary>
public enum PluginStatus
{
    /// <summary>
    /// Plugin is loaded and waiting for the next item.
    /// </summary>
    Idle,

    /// <summary>
    /// Plugin is actively processing an item.
    /// </summary>
    Running,

    /// <summary>
    /// Plugin encountered an error and requires attention.
    /// </summary>
    Faulted
}
