using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrackAndTrace.Core.Interfaces;

namespace TrackAndTrace.Data;

public static class DataServiceExtensions
{
    /// <summary>
    /// Registers the SQLite DbContext and the production repository with the DI container.
    /// </summary>
    public static IServiceCollection AddProductionData(this IServiceCollection services, string databasePath)
    {
        services.AddDbContext<ProductionDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));

        services.AddScoped<IProductionRepository, ProductionRepository>();

        return services;
    }

    /// <summary>
    /// Creates the database schema if it does not already exist.
    /// </summary>
    public static void EnsureDatabaseCreated(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductionDbContext>();
        context.Database.EnsureCreated();
    }
}
