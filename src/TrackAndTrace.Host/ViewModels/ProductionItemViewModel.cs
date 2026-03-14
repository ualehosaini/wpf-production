using CommunityToolkit.Mvvm.ComponentModel;
using TrackAndTrace.Core.Models;

namespace TrackAndTrace.Host.ViewModels;

/// <summary>
/// View model exposing a single production record to the live feed data grid.
/// </summary>
public sealed partial class ProductionItemViewModel : ObservableObject
{
    [ObservableProperty] private string _serialNumber = string.Empty;
    [ObservableProperty] private string _stationId = string.Empty;
    [ObservableProperty] private bool _passed;
    [ObservableProperty] private string _pluginSummary = string.Empty;
    [ObservableProperty] private DateTime _timestamp;

    /// <summary>
    /// Creates a view model instance from a persisted production record.
    /// </summary>
    public static ProductionItemViewModel From(ProductionRecord record) =>
        new()
        {
            SerialNumber = record.SerialNumber,
            StationId = record.StationId,
            Passed = record.Passed,
            PluginSummary = record.PluginSummary,
            Timestamp = record.Timestamp.ToLocalTime()
        };
}
