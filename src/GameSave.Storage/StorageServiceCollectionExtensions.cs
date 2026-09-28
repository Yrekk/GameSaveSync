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

        // Do not touch the filesystem during composition. A broken/missing storage
        // target must remain observable through system status instead of crashing
        // the whole Server before diagnostics are available.
        services.AddSingleton(settings);
        services.AddSingleton<IGameSaveArtifactStorage, LocalGameSaveArtifactStorage>();

        return services;
    }
}
