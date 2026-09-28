using GameSave.Application.MetadataDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameSave.Persistence.Database;

/// <summary>
/// Registers the concrete metadata persistence boundary.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddGameSavePersistence(
        this IServiceCollection services,
        MetadataDatabaseSettings settings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settings);

        services.AddSingleton(settings);
        services.AddSingleton<
            IMetadataDatabaseInspectionProvider,
            SqliteMetadataDatabaseInspectionProvider>();
        services.AddDbContext<GameSaveDbContext>(
            options => options.UseSqlite(
                MetadataDatabaseConnectionStrings.ForOperationalUse(settings)));

        return services;
    }
}
