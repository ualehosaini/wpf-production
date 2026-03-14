using Microsoft.Extensions.Hosting;
using TrackAndTrace.Core.Models;
using TrackAndTrace.Host.ViewModels;

namespace TrackAndTrace.Host.Services;

/// <summary>
/// Drives the production pipeline by emitting one item per configured interval.
/// Each emitted ProductionItem has no serial number yet - ScannerPlugin fills it
/// in via the Scanner TCP server during orchestration.
/// </summary>
internal sealed class ItemSourceService : BackgroundService
{
    private readonly MainViewModel _viewModel;
    private readonly SimulatorSettings _settings;

    /// <summary>
    /// Receives the view model to push items into and the settings controlling the emission rate.
    /// </summary>
    public ItemSourceService(MainViewModel viewModel, SimulatorSettings settings)
    {
        _viewModel = viewModel;
        _settings = settings;
    }

    /// <summary>
    /// Emits a new production item at the configured rate until cancellation is requested.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _viewModel.UpdateStatus("Station active - processing items.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delayMs = (int)(60_000.0 / Math.Max(1, _settings.ItemsPerMinute));
            await Task.Delay(delayMs, stoppingToken).ContinueWith(_ => { });

            if (stoppingToken.IsCancellationRequested) break;

            _viewModel.HandleItem(new ProductionItem
            {
                StationId = _settings.StationId,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
