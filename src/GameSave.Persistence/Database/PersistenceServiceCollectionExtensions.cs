using GameSave.Application.MetadataDatabase;
using GameSave.Application.Profiles;
using GameSave.Persistence.Profiles;
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
        MetadataDatabaseSettings settings,
        MetadataDatabaseControlStoreSettings controlStoreSettings,
        MetadataDatabaseSnapshotSettings snapshotSettings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(controlStoreSettings);
        ArgumentNullException.ThrowIfNull(snapshotSettings);

        services.AddSingleton(settings);
        services.AddSingleton(controlStoreSettings);
        services.AddSingleton(snapshotSettings);
        services.AddSingleton<
            IMetadataDatabaseInspectionProvider,
            SqliteMetadataDatabaseInspectionProvider>();
        services.AddSingleton<
            IMetadataDatabaseClassificationStore,
            XmlMetadataDatabaseClassificationStore>();
        services.AddScoped<IGameProfileRepository, EfGameProfileRepository>();
        services.AddSingleton<
            IMetadataDatabaseLifecycleOperator,
            EfMetadataDatabaseLifecycleOperator>();
        services.AddSingleton<
            IMetadataDatabaseSnapshotStore,
            SqliteMetadataDatabaseSnapshotStore>();
        services.AddDbContext<GameSaveDbContext>(
            options => options.UseSqlite(
                MetadataDatabaseConnectionStrings.ForOperationalUse(settings)));

        return services;
    }
}
