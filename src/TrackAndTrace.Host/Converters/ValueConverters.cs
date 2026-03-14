using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TrackAndTrace.Core.Models;

namespace TrackAndTrace.Host.Converters;

public sealed class BoolToPassFailConverter : IValueConverter
{
    /// <summary>
    /// Returns "PASS" for true, "FAIL" for false.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? "PASS" : "FAIL";

    /// <summary>
    /// Not supported; this converter is one-way.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class BoolToPassFailBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush PassBrush = new(Color.FromRgb(0x40, 0xA0, 0x6A));
    private static readonly SolidColorBrush FailBrush = new(Color.FromRgb(0xD2, 0x4D, 0x6B));

    /// <summary>
    /// Returns the green pass brush for true, or the red fail brush for false.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? PassBrush : FailBrush;

    /// <summary>
    /// Not supported; this converter is one-way.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class PluginStatusToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush IdleBrush = new(Color.FromRgb(0x6C, 0x70, 0x86));
    private static readonly SolidColorBrush RunningBrush = new(Color.FromRgb(0xA6, 0xE3, 0xA1));
    private static readonly SolidColorBrush FaultedBrush = new(Color.FromRgb(0xF3, 0x8B, 0xA8));

    /// <summary>
    /// Returns a brush reflecting the plugin status: green for running, red for faulted, grey for idle.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is PluginStatus status
            ? status switch
            {
                PluginStatus.Running => RunningBrush,
                PluginStatus.Faulted => FaultedBrush,
                _ => IdleBrush
            }
            : IdleBrush;

    /// <summary>
    /// Not supported; this converter is one-way.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class PercentageConverter : IValueConverter
{
    /// <summary>
    /// Formats a double as a percentage string with one decimal place.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is double d ? $"{d:F1}%" : "0.0%";

    /// <summary>
    /// Not supported; this converter is one-way.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
