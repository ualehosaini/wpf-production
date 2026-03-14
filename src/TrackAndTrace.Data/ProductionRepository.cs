using Microsoft.EntityFrameworkCore;
using TrackAndTrace.Core.Interfaces;
using TrackAndTrace.Core.Models;
using TrackAndTrace.Data.Entities;

namespace TrackAndTrace.Data;

public sealed class ProductionRepository : IProductionRepository
{
    private readonly ProductionDbContext _context;

    /// <summary>
    /// Receives the EF Core database context used for all data operations.
    /// </summary>
    public ProductionRepository(ProductionDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Persists a processed production item and its aggregated plugin results as a single database entry.
    /// </summary>
    public async Task SaveAsync(ProductionItem item, IReadOnlyList<ProcessResult> results, CancellationToken cancellationToken = default)
    {
        var passed = results.All(r => r.Passed);
        var summary = string.Join(" | ", results.Select(r => $"{r.PluginName}: {r.Message}"));

        var entry = new ProductionEntry
        {
            SerialNumber = item.SerialNumber,
            StationId = item.StationId,
            Timestamp = item.Timestamp,
            Passed = passed,
            PluginSummary = summary
        };

        _context.ProductionEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Returns the most recent production records ordered by timestamp descending.
    /// </summary>
    public async Task<IReadOnlyList<ProductionRecord>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionEntries
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .Select(e => MapToRecord(e))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns all production records for the current session ordered by timestamp descending.
    /// </summary>
    public async Task<IReadOnlyList<ProductionRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProductionEntries
            .OrderByDescending(e => e.Timestamp)
            .Select(e => MapToRecord(e))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Maps a database entity to the domain model returned by the repository.
    /// </summary>
    private static ProductionRecord MapToRecord(ProductionEntry e) =>
        new()
        {
            Id = e.Id,
            SerialNumber = e.SerialNumber,
            StationId = e.StationId,
            Timestamp = e.Timestamp,
            Passed = e.Passed,
            PluginSummary = e.PluginSummary
        };
}
