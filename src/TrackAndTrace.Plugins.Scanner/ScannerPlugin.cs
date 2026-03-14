using System.Net.Sockets;
using System.Text;
using TrackAndTrace.Core.Interfaces;
using TrackAndTrace.Core.Models;

namespace TrackAndTrace.Plugins.Scanner;

/// <summary>
/// Sends a SCAN request to the Scanner TCP server and reads back the barcode result.
/// Maintains a persistent connection; reconnects on fault.
/// In a real deployment this talks to the barcode / RFID reader controller API instead.
/// </summary>
public sealed class ScannerPlugin : IStationPlugin, IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private TcpClient? _client;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Display name of the plugin shown in the operator dashboard.
    /// </summary>
    public string Name => "Scanner";

    /// <summary>
    /// Current operational state of the scanner plugin.
    /// </summary>
    public PluginStatus Status { get; private set; } = PluginStatus.Idle;

    /// <summary>
    /// Reads the scanner host and port from hardware settings.
    /// </summary>
    public ScannerPlugin(HardwareSettings settings)
    {
        _host = settings.ScannerHost;
        _port = settings.ScannerPort;
    }

    /// <summary>
    /// Sends a SCAN command to the TCP server, reads the barcode response, and updates the item serial number.
    /// </summary>
    public ProcessResult ProcessItem(ProductionItem item)
    {
        _lock.Wait();
        Status = PluginStatus.Running;
        try
        {
            EnsureConnected();

            _writer!.WriteLine("SCAN");
            _writer.Flush();

            var response = _reader!.ReadLine() ?? "READ_ERROR";
            var isError = response.Equals("READ_ERROR", StringComparison.OrdinalIgnoreCase);

            item.SerialNumber = response;

            Status = PluginStatus.Idle;
            return new ProcessResult
            {
                PluginName = Name,
                Passed     = !isError,
                Message    = isError
                    ? "Barcode unreadable - scanner returned READ_ERROR"
                    : $"Barcode verified: {response}"
            };
        }
        catch (Exception ex)
        {
            DropConnection();
            Status = PluginStatus.Faulted;
            return new ProcessResult
            {
                PluginName = Name,
                Passed     = false,
                Message    = $"Scanner unreachable: {ex.Message}"
            };
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Required by IStationPlugin. This plugin does not use runtime configuration.
    /// </summary>
    public void Configure(PluginConfiguration config) { }

    /// <summary>
    /// Opens a new TCP connection if one is not already active.
    /// </summary>
    private void EnsureConnected()
    {
        if (_client?.Connected == true) return;
        DropConnection();

        _client = new TcpClient { NoDelay = true, ReceiveTimeout = 5_000, SendTimeout = 5_000 };
        _client.Connect(_host, _port);

        var stream = _client.GetStream();
        _writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true) { AutoFlush = false };
        _reader = new StreamReader(stream, new UTF8Encoding(false), leaveOpen: true);
    }

    /// <summary>
    /// Disposes the active TCP stream and client, resetting connection state.
    /// </summary>
    private void DropConnection()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        _client?.Dispose();
        _writer = null;
        _reader = null;
        _client = null;
    }

    /// <summary>
    /// Drops the TCP connection and releases the semaphore.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DropConnection();
        _lock.Dispose();
    }
}
