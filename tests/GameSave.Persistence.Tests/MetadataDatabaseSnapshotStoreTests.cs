using GameSave.Application.MetadataDatabase;
using GameSave.Persistence.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Tests;

public sealed class MetadataDatabaseSnapshotStoreTests
{
    [Fact]
    public async Task CreateRollingSnapshot_IsValidAndRetainsTwo()
    {
        using var fixture = await SnapshotFixture.CreateAsync();
        var store = fixture.CreateStore();

        var first = await store.CreateAsync(MetadataDatabaseSnapshotKind.Rolling);
        var second = await store.CreateAsync(MetadataDatabaseSnapshotKind.Rolling);
        var third = await store.CreateAsync(MetadataDatabaseSnapshotKind.Rolling);

        Assert.True(first.IsValid);
        Assert.True(second.IsValid);
        Assert.True(third.IsValid);

        var rolling = (await store.ListAsync())
            .Where(item => item.Kind == MetadataDatabaseSnapshotKind.Rolling)
            .ToArray();

        Assert.Equal(2, rolling.Length);
    }

    [Fact]
    public async Task Restore_UsesExplicitValidatedSnapshot()
    {
        using var fixture = await SnapshotFixture.CreateAsync();
        var store = fixture.CreateStore();
        var snapshot = await store.CreateAsync(
            MetadataDatabaseSnapshotKind.Rolling);

        await File.WriteAllTextAsync(
            fixture.DatabasePath,
            "broken-active-database");

        Assert.True(await store.RestoreAsync(snapshot.Id));

        var options = new DbContextOptionsBuilder<GameSaveDbContext>()
            .UseSqlite($"Data Source={fixture.DatabasePath};Pooling=False")
            .Options;

        await using var context = new GameSaveDbContext(options);
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
    }

    private sealed class SnapshotFixture : IDisposable
    {
        private readonly string _root;
        private readonly MetadataDatabaseSettings _databaseSettings;
        private readonly MetadataDatabaseSnapshotSettings _snapshotSettings;

        private SnapshotFixture(
            string root,
            MetadataDatabaseSettings databaseSettings,
            MetadataDatabaseSnapshotSettings snapshotSettings)
        {
            _root = root;
            _databaseSettings = databaseSettings;
            _snapshotSettings = snapshotSettings;
        }

        public string DatabasePath => _databaseSettings.DatabasePath;

        public static async Task<SnapshotFixture> CreateAsync()
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "GameSaveSync.Tests",
                Guid.NewGuid().ToString("N"));

            var databaseSettings = MetadataDatabaseSettings.FromConfiguredPath(
                "data/metadata.db",
                root);
            var snapshotSettings = MetadataDatabaseSnapshotSettings.FromConfiguredPath(
                "snapshots",
                root);

            Directory.CreateDirectory(
                Path.GetDirectoryName(databaseSettings.DatabasePath)!);

            var options = new DbContextOptionsBuilder<GameSaveDbContext>()
                .UseSqlite(
                    MetadataDatabaseConnectionStrings.ForExplicitInitialization(
                        databaseSettings))
                .Options;

            await using var context = new GameSaveDbContext(options);
            await context.Database.MigrateAsync();

            return new SnapshotFixture(
                root,
                databaseSettings,
                snapshotSettings);
        }

        public SqliteMetadataDatabaseSnapshotStore CreateStore()
        {
            return new SqliteMetadataDatabaseSnapshotStore(
                _databaseSettings,
                _snapshotSettings);
        }

        public void Dispose()
        {
            using (var poolMarker = new SqliteConnection(
                MetadataDatabaseConnectionStrings.ForOperationalUse(
                    _databaseSettings)))
            {
                SqliteConnection.ClearPool(poolMarker);
            }

            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
    }
}
