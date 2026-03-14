using CommunityToolkit.Mvvm.ComponentModel;
using TrackAndTrace.Core.Models;

namespace TrackAndTrace.Host.ViewModels;

/// <summary>
/// View model representing the live operational status of a single station plugin.
/// </summary>
public sealed partial class PluginStatusViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private PluginStatus _status;
}
