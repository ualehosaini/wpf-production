using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TrackAndTrace.Core.Interfaces;
using TrackAndTrace.Core.Models;
using TrackAndTrace.Data;
using TrackAndTrace.Host.Services;
using TrackAndTrace.Host.ViewModels;
using TrackAndTrace.Host.Views;
using TrackAndTrace.Plugins.Inspection;
using TrackAndTrace.Plugins.Scanner;
using GenericHost = Microsoft.Extensions.Hosting.Host;
using IGenericHost = Microsoft.Extensions.Hosting.IHost;

namespace TrackAndTrace.Host;

/// <summary>
/// Entry point and composition root for the WPF application.
/// </summary>
public partial class App : Application
{
    private IGenericHost? _host;

    /// <summary>
    /// Bootstraps the DI container, creates the database, starts the background host, and shows the main window.
    /// </summary>
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TrackAndTrace", "production.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _host = GenericHost.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                var simulatorSettings = new SimulatorSettings();
                var hardwareSettings = new HardwareSettings();
                services.AddSingleton(simulatorSettings);
                services.AddSingleton(hardwareSettings);
                services.AddSingleton<IStationPlugin, ScannerPlugin>();
                services.AddSingleton<IStationPlugin, InspectionPlugin>();
                services.AddSingleton<PluginOrchestrator>();
                services.AddHostedService<ItemSourceService>();
                services.AddProductionData(dbPath);
                services.AddSingleton<ErpExportService>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
        DataServiceExtensions.EnsureDatabaseCreated(_host.Services);

        _host.Services.GetRequiredService<MainViewModel>();

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }

    /// <summary>
    /// Stops and disposes the background host before the application exits.
    /// </summary>
    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}

