using GameSave.Application.MetadataDatabase;
using GameSave.Persistence.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Tests;

public sealed class MetadataDatabaseInspectionProviderTests
{
    private const string CurrentMigration =
        "20260928123000_AddGameProfiles";

    [Fact]
    public async Task Missing_DoesNotCreateDatabase()
    {
        using var fixture = CreateFixture();
        var provider = CreateProvider(fixture);

        var inspection = await provider.InspectAsync();

        Assert.Equal(MetadataDatabaseState.Missing, inspection.SuggestedState);
        Assert.Equal([MetadataDatabaseState.Missing], inspection.CandidateStates);
        Assert.False(File.Exists(fixture.Settings.DatabasePath));
    }

    [Fact]
    public async Task EmptyValidDatabase_SuggestsUninitializedAndAllowsInvalid()
    {
        using var fixture = CreateFixture();
        await fixture.CreateEmptyDatabaseAsync();

        var provider = CreateProvider(fixture);
        var inspection = await provider.InspectAsync();

        Assert.Equal(MetadataDatabaseState.Uninitialized, inspection.SuggestedState);
        Assert.Equal(
            [MetadataDatabaseState.Uninitialized, MetadataDatabaseState.Invalid],
            inspection.CandidateStates);
        Assert.True(inspection.RequiresAdministratorClassification);
        Assert.Equal(0, inspection.Facts.UserTableCount);
        Assert.Equal(0, inspection.Facts.AppliedMigrationCount);
        Assert.Contains(
            inspection.Findings,
            finding =>
                finding.Code == MetadataDatabaseFindingCodes.UserObjectsAbsent);

        var tableNames = await fixture.ReadTableNamesAsync();
        Assert.Empty(tableNames);
    }

    [Fact]
    public async Task DatabaseWithForeignTable_SuggestsInvalidButAllowsUninitialized()
    {
        using var fixture = CreateFixture();
        await fixture.ExecuteSqlAsync(
            "CREATE TABLE ExternalData (Id INTEGER PRIMARY KEY);");

        var provider = CreateProvider(fixture);
        var inspection = await provider.InspectAsync();

        Assert.Equal(MetadataDatabaseState.Invalid, inspection.SuggestedState);
        Assert.Equal(
            [MetadataDatabaseState.Uninitialized, MetadataDatabaseState.Invalid],
            inspection.CandidateStates);
        Assert.Equal(1, inspection.Facts.UserTableCount);
        var finding = Assert.Single(
            inspection.Findings,
            finding =>
                finding.Code ==
                MetadataDatabaseFindingCodes.NonApplicationObjectsPresent);
        Assert.Equal(1, finding.Details["object_count"]);
    }

    [Fact]
    public async Task AllKnownMigrationsApplied_IsDeterministicallyReady()
    {
        using var fixture = CreateFixture();
        await fixture.ApplyKnownMigrationsAsync();

        var provider = CreateProvider(fixture);
        var inspection = await provider.InspectAsync();

        Assert.Equal(MetadataDatabaseState.Ready, inspection.SuggestedState);
        Assert.Equal(
            [MetadataDatabaseState.Ready],
            inspection.CandidateStates);
        Assert.False(inspection.RequiresAdministratorClassification);
        Assert.Equal(CurrentMigration, inspection.Facts.CurrentMigration);
        Assert.Equal(CurrentMigration, inspection.Facts.TargetMigration);
    }

    [Fact]
    public async Task UnknownAppliedMigration_SuggestsTooNew()
    {
        using var fixture = CreateFixture();
        await fixture.ApplyKnownMigrationsAsync();
        await fixture.ExecuteSqlAsync(
            """
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260929000000_FutureMigration', '10.0.12');
            """);

        var provider = CreateProvider(fixture);
        var inspection = await provider.InspectAsync();

        Assert.Equal(MetadataDatabaseState.TooNew, inspection.SuggestedState);
        Assert.Equal(
            [MetadataDatabaseState.TooNew, MetadataDatabaseState.Invalid],
            inspection.CandidateStates);
    }

    [Fact]
    public async Task IncoherentMigrationHistory_IsInvalidWithoutRewritingIntegrityFact()
    {
        using var fixture = CreateFixture();
        await fixture.ExecuteSqlAsync(
            """
            CREATE TABLE "__EFMigrationsHistory" (
                WrongColumn TEXT NOT NULL
            );
            """);

        var inspection = await CreateProvider(fixture).InspectAsync();

        Assert.Equal(MetadataDatabaseState.Invalid, inspection.SuggestedState);
        Assert.True(inspection.Facts.IntegrityValid);
    }

    [Fact]
    public async Task CorruptDatabase_IsInvalid()
    {
        using var fixture = CreateFixture();
        Directory.CreateDirectory(Path.GetDirectoryName(fixture.Settings.DatabasePath)!);
        await File.WriteAllTextAsync(
            fixture.Settings.DatabasePath,
            "this is not sqlite");

        var provider = CreateProvider(fixture);
        var inspection = await provider.InspectAsync();

        Assert.Equal(MetadataDatabaseState.Invalid, inspection.SuggestedState);
        Assert.Equal([MetadataDatabaseState.Invalid], inspection.CandidateStates);
    }

    [Fact]
    public async Task DatabasePathOccupiedByDirectory_IsUnavailable()
    {
        using var fixture = CreateFixture();
        Directory.CreateDirectory(fixture.Settings.DatabasePath);

        var provider = CreateProvider(fixture);
        var inspection = await provider.InspectAsync();

        Assert.Equal(MetadataDatabaseState.Unavailable, inspection.SuggestedState);
        Assert.True(inspection.Facts.PathOccupiedByNonFile);
    }

    [Fact]
    public async Task ReopenClassification_IsDeterministic()
    {
        using var fixture = CreateFixture();
        await fixture.ApplyKnownMigrationsAsync();

        var first = await CreateProvider(fixture).InspectAsync();
        var second = await CreateProvider(fixture).InspectAsync();

        Assert.Equal(first.SuggestedState, second.SuggestedState);
        Assert.Equal(first.CandidateStates, second.CandidateStates);
        Assert.Equal(first.Facts, second.Facts);
    }

    [Fact]
    public async Task ReopenRevision_IsDeterministic()
    {
        using var fixture = CreateFixture();
        await fixture.ApplyKnownMigrationsAsync();

        var first = await CreateProvider(fixture).InspectAsync();
        var second = await CreateProvider(fixture).InspectAsync();

        Assert.Equal(first.Context.Revision, second.Context.Revision);
        Assert.Equal(
            $"metadata-database:{Path.GetFullPath(fixture.Settings.DatabasePath)}",
            first.Context.ResourceIdentity);
        Assert.Equal(
            MetadataDatabaseClassificationPolicy.CurrentVersion,
            first.Context.ClassificationPolicyVersion);
    }

    [Fact]
    public async Task SchemaChange_ChangesInspectionRevision()
    {
        using var fixture = CreateFixture();
        await fixture.CreateEmptyDatabaseAsync();

        var before = await CreateProvider(fixture).InspectAsync();

        await fixture.ExecuteSqlAsync(
            "CREATE TABLE ExternalData (Id INTEGER PRIMARY KEY);");

        var after = await CreateProvider(fixture).InspectAsync();

        Assert.NotEqual(before.Context.Revision, after.Context.Revision);
    }

    [Fact]
    public async Task DataChangeWithoutSchemaChange_DoesNotChangeInspectionRevision()
    {
        using var fixture = CreateFixture();
        await fixture.ExecuteSqlAsync(
            "CREATE TABLE ExternalData (Id INTEGER PRIMARY KEY, Name TEXT);");

        var before = await CreateProvider(fixture).InspectAsync();

        await fixture.ExecuteSqlAsync(
            "INSERT INTO ExternalData (Name) VALUES ('example');");

        var after = await CreateProvider(fixture).InspectAsync();

        Assert.Equal(before.Context.Revision, after.Context.Revision);
    }

    private static SqliteMetadataDatabaseInspectionProvider CreateProvider(
        TestDatabaseFixture fixture)
    {
        return new SqliteMetadataDatabaseInspectionProvider(
            fixture.Settings,
            TestConnectionString(
                MetadataDatabaseConnectionStrings.ForOperationalUse(
                    fixture.Settings)));
    }

    private static string TestConnectionString(string connectionString)
    {
        return new SqliteConnectionStringBuilder(connectionString)
        {
            Pooling = false,
        }.ToString();
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

        public async Task CreateEmptyDatabaseAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Settings.DatabasePath)!);

            await using var connection = new SqliteConnection(
                TestConnectionString(
                    MetadataDatabaseConnectionStrings
                        .ForExplicitInitialization(Settings)));

            await connection.OpenAsync();
        }

        public async Task ExecuteSqlAsync(string sql)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Settings.DatabasePath)!);

            await using var connection = new SqliteConnection(
                TestConnectionString(
                    MetadataDatabaseConnectionStrings
                        .ForExplicitInitialization(Settings)));

            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        public async Task<IReadOnlyList<string>> ReadTableNamesAsync()
        {
            await using var connection = new SqliteConnection(
                TestConnectionString(
                    MetadataDatabaseConnectionStrings
                        .ForOperationalUse(Settings)));

            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT name
                FROM sqlite_master
                WHERE type = 'table'
                  AND name NOT LIKE 'sqlite_%'
                ORDER BY name;
                """;

            await using var reader = await command.ExecuteReaderAsync();
            var names = new List<string>();

            while (await reader.ReadAsync())
            {
                names.Add(reader.GetString(0));
            }

            return names;
        }

        public async Task ApplyKnownMigrationsAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Settings.DatabasePath)!);

            var options = new DbContextOptionsBuilder<GameSaveDbContext>()
                .UseSqlite(
                    TestConnectionString(
                        MetadataDatabaseConnectionStrings
                            .ForExplicitInitialization(Settings)))
                .Options;

            await using var context = new GameSaveDbContext(options);
            await context.Database.MigrateAsync();
        }

        public void Dispose()
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, recursive: true);
            }
        }


    }
}
