using GameSave.Persistence.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Tests;

public sealed class MetadataMigrationTests
{
    private const string InitialMigration =
        "20260928060000_InitialMetadataDatabase";

    [Fact]
    public async Task OperationalConnection_DoesNotCreateMissingDatabase()
    {
        var fixture = CreateFixture();

        try
        {
            var options = BuildOptions(
                MetadataDatabaseConnectionStrings.ForOperationalUse(fixture.Settings));

            await using var context = new GameSaveDbContext(options);

            Assert.False(await context.Database.CanConnectAsync());
            Assert.False(File.Exists(fixture.Settings.DatabasePath));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task ExplicitMaintenance_CanApplyBaselineAndLeaveNoPendingMigrations()
    {
        var fixture = CreateFixture();

        try
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(fixture.Settings.DatabasePath)!);

            var maintenanceOptions = BuildOptions(
                MetadataDatabaseConnectionStrings.ForExplicitMaintenance(
                    fixture.Settings));

            await using (var context = new GameSaveDbContext(maintenanceOptions))
            {
                var pendingBefore = await context.Database.GetPendingMigrationsAsync();

                Assert.Contains(InitialMigration, pendingBefore);

                await context.Database.MigrateAsync();

                var applied = await context.Database.GetAppliedMigrationsAsync();
                var pendingAfter = await context.Database.GetPendingMigrationsAsync();

                Assert.Contains(InitialMigration, applied);
                Assert.Empty(pendingAfter);
            }

            var operationalOptions = BuildOptions(
                MetadataDatabaseConnectionStrings.ForOperationalUse(
                    fixture.Settings));

            await using var reopened = new GameSaveDbContext(operationalOptions);

            Assert.True(await reopened.Database.CanConnectAsync());
            Assert.Empty(await reopened.Database.GetPendingMigrationsAsync());
        }
        finally
        {
            fixture.Dispose();
        }
    }

    private static DbContextOptions<GameSaveDbContext> BuildOptions(
        string connectionString)
    {
        // Temporary SQLite test databases disable pooling locally so their
        // cleanup cannot disturb unrelated SQLite tests running in parallel.
        var testConnectionString = new SqliteConnectionStringBuilder(connectionString)
        {
            Pooling = false,
        }.ToString();

        return new DbContextOptionsBuilder<GameSaveDbContext>()
            .UseSqlite(testConnectionString)
            .Options;
    }

    private static TestDatabaseFixture CreateFixture()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "GameSaveSync.Tests",
            Guid.NewGuid().ToString("N"));

        var settings = MetadataDatabaseSettings.FromConfiguredPath(
            Path.Combine("data", "metadata.db"),
            root);

        return new TestDatabaseFixture(root, settings);
    }

    private sealed class TestDatabaseFixture(
        string rootPath,
        MetadataDatabaseSettings settings) : IDisposable
    {
        public MetadataDatabaseSettings Settings { get; } = settings;

        public void Dispose()
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, recursive: true);
            }
        }
    }
}
