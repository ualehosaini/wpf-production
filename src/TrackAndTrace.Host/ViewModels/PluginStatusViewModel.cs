using CommunityToolkit.Mvvm.ComponentModel;
using TrackAndTrace.Core.Models;

namespace TrackAndTrace.Host.ViewModels;

public sealed partial class PluginStatusViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private PluginStatus _status;
}
