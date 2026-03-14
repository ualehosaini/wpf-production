using System.Globalization;
using System.IO;
using TrackAndTrace.Core.Interfaces;

namespace TrackAndTrace.Host.Services;

public sealed class ErpExportService
{
    private readonly IProductionRepository _repository;

    /// <summary>
    /// Receives the repository used to query production records for export.
    /// </summary>
    public ErpExportService(IProductionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Writes all production records to a UTF-8 CSV file at the specified path.
    /// </summary>
    public async Task ExportToCsvAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var records = await _repository.GetAllAsync(cancellationToken);

        await using var writer = new StreamWriter(filePath, append: false, System.Text.Encoding.UTF8);
        await writer.WriteLineAsync("Id,SerialNumber,StationId,Timestamp,Passed,PluginSummary");

        foreach (var record in records)
        {
            var line = string.Join(",",
                record.Id,
                EscapeCsv(record.SerialNumber),
                EscapeCsv(record.StationId),
                record.Timestamp.ToString("o", CultureInfo.InvariantCulture),
                record.Passed,
                EscapeCsv(record.PluginSummary));

            await writer.WriteLineAsync(line);
        }
    }

    /// <summary>
    /// Wraps a field in double quotes if the value contains commas, quotes, or newlines.
    /// </summary>
    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
