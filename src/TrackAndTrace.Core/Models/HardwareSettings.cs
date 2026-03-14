namespace TrackAndTrace.Core.Models;

public sealed class HardwareSettings
{
    public string ScannerHost { get; set; } = "127.0.0.1";
    public int ScannerPort { get; set; } = 5001;
    public string VisionHost { get; set; } = "127.0.0.1";
    public int VisionPort { get; set; } = 5002;
    public int ReconnectDelaySeconds { get; set; } = 5;
}
