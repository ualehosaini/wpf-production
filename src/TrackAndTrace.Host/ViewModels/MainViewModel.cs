using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TrackAndTrace.Core.Interfaces;
using TrackAndTrace.Core.Models;
using TrackAndTrace.Host.Services;

namespace TrackAndTrace.Host.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private const int MaxDisplayedItems = 200;

    private readonly PluginOrchestrator _orchestrator;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ErpExportService _exportService;

    [ObservableProperty] private int _totalItems;
    [ObservableProperty] private int _passedItems;
    [ObservableProperty] private int _failedItems;
    [ObservableProperty] private double _rejectRate;
    [ObservableProperty] private string _stationId = string.Empty;
    [ObservableProperty] private string _statusMessage = "Station initializing.";
    [ObservableProperty] private string _scannerAddress = string.Empty;
    [ObservableProperty] private string _visionAddress = string.Empty;

    public ObservableCollection<ProductionItemViewModel> RecentItems { get; } = new();
    public ObservableCollection<PluginStatusViewModel> PluginStatuses { get; } = new();

    /// <summary>
    /// Initialises the view model, wires hardware address strings, and seeds the plugin status list.
    /// </summary>
    public MainViewModel(
        PluginOrchestrator orchestrator,
        IServiceScopeFactory scopeFactory,
        ErpExportService exportService,
        HardwareSettings hardware,
        SimulatorSettings simulator)
    {
        _orchestrator = orchestrator;
        _scopeFactory = scopeFactory;
        _exportService = exportService;

        _stationId = simulator.StationId;
        _scannerAddress = $"{hardware.ScannerHost}:{hardware.ScannerPort}";
        _visionAddress = $"{hardware.VisionHost}:{hardware.VisionPort}";

        foreach (var plugin in orchestrator.Plugins)
            PluginStatuses.Add(new PluginStatusViewModel { Name = plugin.Name, Status = plugin.Status });
    }

    /// <summary>
    /// Posts a status bar message on the UI thread.
    /// </summary>
    internal void UpdateStatus(string message)
    {
        Application.Current.Dispatcher.InvokeAsync(() => StatusMessage = message);
    }

    /// <summary>
    /// Processes a scanned production item through all plugins and updates the live feed.
    /// </summary>
    internal void HandleItem(ProductionItem item)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            for (int i = 0; i < _orchestrator.Plugins.Count; i++)
                PluginStatuses[i].Status = PluginStatus.Running;
        });

        var results = _orchestrator.Process(item);
        var passed = results.All(r => r.Passed);

        Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            TotalItems++;
            if (passed) PassedItems++;
            else FailedItems++;
            RejectRate = TotalItems > 0 ? (double)FailedItems / TotalItems * 100 : 0;

            var vm = new ProductionItemViewModel
            {
                SerialNumber = item.SerialNumber,
                StationId = item.StationId,
                Passed = passed,
                PluginSummary = string.Join(" | ", results.Select(r => r.Message)),
                Timestamp = item.Timestamp.ToLocalTime()
            };

            RecentItems.Insert(0, vm);
            if (RecentItems.Count > MaxDisplayedItems)
                RecentItems.RemoveAt(RecentItems.Count - 1);

            await PersistAsync(item, results);
        });

        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            for (int i = 0; i < _orchestrator.Plugins.Count; i++)
                PluginStatuses[i].Status = _orchestrator.Plugins[i].Status;
        }, DispatcherPriority.Background);
    }

    /// <summary>
    /// Saves a processed item and its plugin results to the database.
    /// </summary>
    private async Task PersistAsync(ProductionItem item, IReadOnlyList<Core.Models.ProcessResult> results)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductionRepository>();
        await repo.SaveAsync(item, results);
    }

    /// <summary>
    /// Prompts for a file path and exports the current session's production data as CSV.
    /// </summary>
    [RelayCommand]
    private async Task ExportToCsvAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"production-export-{DateTime.Now:yyyyMMdd-HHmmss}.csv"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            await _exportService.ExportToCsvAsync(dialog.FileName);
            StatusMessage = $"Exported to {dialog.FileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
    }
}
