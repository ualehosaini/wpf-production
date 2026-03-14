using TrackAndTrace.Core.Models;

namespace TrackAndTrace.Core.Interfaces;

public interface IProductionRepository
{
    /// <summary>
    /// Persists a production item and its plugin results.
    /// </summary>
    Task SaveAsync(ProductionItem item, IReadOnlyList<ProcessResult> results, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the most recent production records up to the specified count.
    /// </summary>
    Task<IReadOnlyList<ProductionRecord>> GetRecentAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every record in the production log.
    /// </summary>
    Task<IReadOnlyList<ProductionRecord>> GetAllAsync(CancellationToken cancellationToken = default);
}
