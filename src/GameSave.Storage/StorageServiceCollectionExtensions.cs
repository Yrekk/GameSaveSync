using GameSave.Application.Storage;
using GameSave.Storage.Local;
using Microsoft.Extensions.DependencyInjection;

namespace GameSave.Storage;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddGameSaveStorage(
        this IServiceCollection services,
        LocalSaveArtifactStorageSettings settings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settings);

        // Storage root creation is infrastructure bootstrap, not save publication.
        // Creating it here lets read-only system status check a stable configured root
        // without turning a GET status request into a filesystem mutation.
        Directory.CreateDirectory(settings.RootPath);

        services.AddSingleton(settings);
        services.AddSingleton<IGameSaveArtifactStorage, LocalGameSaveArtifactStorage>();

        return services;
    }
}
