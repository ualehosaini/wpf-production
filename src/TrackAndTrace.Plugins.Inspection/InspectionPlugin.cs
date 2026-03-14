using System.Net.Sockets;
using System.Text;
using TrackAndTrace.Core.Interfaces;
using TrackAndTrace.Core.Models;

namespace TrackAndTrace.Plugins.Inspection;

/// <summary>
/// Sends the serial number to the VisionOrCamera TCP server and returns its
/// PASS / FAIL response. Maintains a persistent connection; reconnects on fault.
/// In a real deployment this talks to the camera controller API instead.
/// </summary>
public sealed class InspectionPlugin : IStationPlugin, IDisposable
{
    private readonly string _host;
    private readonly int _port;

    private TcpClient? _client;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _disposed;

    public string Name => "Inspection";
    public PluginStatus Status { get; private set; } = PluginStatus.Idle;

    /// <summary>
    /// Reads the vision system host and port from hardware settings.
    /// </summary>
    public InspectionPlugin(HardwareSettings settings)
    {
        _host = settings.VisionHost;
        _port = settings.VisionPort;
    }

    /// <summary>
    /// Sends the serial number to the vision TCP server and returns a pass or fail result.
    /// </summary>
    public ProcessResult ProcessItem(ProductionItem item)
    {
        _lock.Wait();
        Status = PluginStatus.Running;
        try
        {
            EnsureConnected();

            _writer!.WriteLine(item.SerialNumber);
            _writer.Flush();

            var response = _reader!.ReadLine() ?? "FAIL:No response from vision system";
            var passed = response.StartsWith("PASS", StringComparison.OrdinalIgnoreCase);

            var message = passed
                ? $"Visual inspection passed: {item.SerialNumber}"
                : response.Replace("FAIL:", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

            Status = PluginStatus.Idle;
            return new ProcessResult { PluginName = Name, Passed = passed, Message = message };
        }
        catch (Exception ex)
        {
            DropConnection();
            Status = PluginStatus.Faulted;
            return new ProcessResult
            {
                PluginName = Name,
                Passed = false,
                Message = $"Vision system unreachable: {ex.Message}"
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
