namespace TrackAndTrace.Core.Models;

/// <summary>
/// Contains network addresses and timing settings for physical hardware devices.
/// </summary>
public sealed class HardwareSettings
{
    /// <summary>
    /// Hostname or IP address of the barcode/RFID scanner service.
    /// </summary>
    public string ScannerHost { get; set; } = "127.0.0.1";

    /// <summary>
    /// TCP port used to connect to the scanner service.
    /// </summary>
    public int ScannerPort { get; set; } = 5001;

    /// <summary>
    /// Hostname or IP address of the vision/camera inspection service.
    /// </summary>
    public string VisionHost { get; set; } = "127.0.0.1";

    /// <summary>
    /// TCP port used to connect to the vision service.
    /// </summary>
    public int VisionPort { get; set; } = 5002;

    /// <summary>
    /// Seconds to wait before reattempting a failed hardware connection.
    /// </summary>
    public int ReconnectDelaySeconds { get; set; } = 5;
}
