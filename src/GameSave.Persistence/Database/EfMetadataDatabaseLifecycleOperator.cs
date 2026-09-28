using GameSave.Application.MetadataDatabase;
using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Database;

internal sealed class EfMetadataDatabaseLifecycleOperator(
    MetadataDatabaseSettings settings)
    : IMetadataDatabaseLifecycleOperator
{
    private readonly MetadataDatabaseSettings _settings =
        settings ?? throw new ArgumentNullException(nameof(settings));

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_settings.DatabasePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var options = new DbContextOptionsBuilder<GameSaveDbContext>()
            .UseSqlite(
                MetadataDatabaseConnectionStrings.ForExplicitInitialization(
                    _settings))
            .Options;

        await using var context = new GameSaveDbContext(options);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public async Task ApplyPendingMigrationsAsync(
        CancellationToken cancellationToken = default)
    {
        var options = new DbContextOptionsBuilder<GameSaveDbContext>()
            .UseSqlite(
                MetadataDatabaseConnectionStrings.ForOperationalUse(_settings))
            .Options;

        await using var context = new GameSaveDbContext(options);
        await context.Database.MigrateAsync(cancellationToken);
    }
}
