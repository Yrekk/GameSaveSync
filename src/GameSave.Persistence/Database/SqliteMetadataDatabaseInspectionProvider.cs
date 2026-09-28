using GameSave.Application.MetadataDatabase;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Database;

/// <summary>
/// SQLite/EF implementation of read-only metadata database inspection.
/// </summary>
internal sealed class SqliteMetadataDatabaseInspectionProvider(
    MetadataDatabaseSettings settings)
    : IMetadataDatabaseInspectionProvider
{
    private const string EfMigrationHistoryTable = "__EFMigrationsHistory";

    private readonly MetadataDatabaseSettings _settings =
        settings ?? throw new ArgumentNullException(nameof(settings));

    public async Task<MetadataDatabaseInspection> InspectAsync(
        CancellationToken cancellationToken = default)
    {
        var knownMigrations = GetKnownMigrations();
        var targetMigration = knownMigrations.LastOrDefault();
        var databasePath = _settings.DatabasePath;

        if (!File.Exists(databasePath))
        {
            var occupiedByNonFile = Directory.Exists(databasePath);
            var state = occupiedByNonFile
                ? MetadataDatabaseState.Unavailable
                : MetadataDatabaseState.Missing;

            return new MetadataDatabaseInspection(
                new MetadataDatabaseInspectionFacts(
                    FileExists: false,
                    PathOccupiedByNonFile: occupiedByNonFile,
                    Accessible: false,
                    IntegrityValid: null,
                    HasMigrationHistoryTable: null,
                    AppliedMigrationCount: null,
                    UserTableCount: null,
                    CurrentMigration: null,
                    TargetMigration: targetMigration),
                [state],
                state,
                [
                    occupiedByNonFile
                        ? "The configured database path is occupied by a directory or another non-file resource."
                        : "The configured metadata database file does not exist.",
                ]);
        }

        await using var connection = new SqliteConnection(
            MetadataDatabaseConnectionStrings.ForOperationalUse(_settings));

        try
        {
            await connection.OpenAsync(cancellationToken);
        }
        catch (SqliteException exception)
        {
            return BuildUnavailable(
                targetMigration,
                $"SQLite could not open the existing metadata database: error {exception.SqliteErrorCode}.");
        }
        catch (IOException)
        {
            return BuildUnavailable(
                targetMigration,
                "The existing metadata database file could not be opened because of an I/O failure.");
        }
        catch (UnauthorizedAccessException)
        {
            return BuildUnavailable(
                targetMigration,
                "The existing metadata database file could not be opened because access was denied.");
        }

        try
        {
            if (!await CheckIntegrityAsync(connection, cancellationToken))
            {
                return BuildInvalid(
                    targetMigration,
                    "SQLite integrity checking reported corruption or structural inconsistency.");
            }

            var tableNames = await ReadTableNamesAsync(connection, cancellationToken);
            var hasMigrationHistoryTable = tableNames.Contains(
                EfMigrationHistoryTable,
                StringComparer.Ordinal);
            var userTableCount = tableNames.Count(
                tableName => !string.Equals(
                    tableName,
                    EfMigrationHistoryTable,
                    StringComparison.Ordinal));

            var appliedMigrations = hasMigrationHistoryTable
                ? await ReadAppliedMigrationsAsync(connection, cancellationToken)
                : [];

            var facts = new MetadataDatabaseInspectionFacts(
                FileExists: true,
                PathOccupiedByNonFile: false,
                Accessible: true,
                IntegrityValid: true,
                HasMigrationHistoryTable: hasMigrationHistoryTable,
                AppliedMigrationCount: appliedMigrations.Count,
                UserTableCount: userTableCount,
                CurrentMigration: appliedMigrations.LastOrDefault(),
                TargetMigration: targetMigration);

            return MetadataDatabaseInspectionClassifier.ClassifyAccessibleDatabase(
                facts,
                knownMigrations,
                appliedMigrations);
        }
        catch (SqliteException exception) when (IsTransientAvailabilityError(exception))
        {
            return BuildUnavailable(
                targetMigration,
                $"SQLite became temporarily unavailable during inspection: error {exception.SqliteErrorCode}.");
        }
        catch (SqliteException exception)
        {
            return BuildInvalid(
                targetMigration,
                $"The SQLite database or GameSaveSync migration history could not be read coherently: error {exception.SqliteErrorCode}.");
        }
    }

    private IReadOnlyList<string> GetKnownMigrations()
    {
        var options = new DbContextOptionsBuilder<GameSaveDbContext>()
            .UseSqlite(
                MetadataDatabaseConnectionStrings.ForOperationalUse(_settings))
            .Options;

        using var context = new GameSaveDbContext(options);
        return context.Database.GetMigrations().ToArray();
    }

    private static async Task<bool> CheckIntegrityAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA quick_check;";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var sawResult = false;

        while (await reader.ReadAsync(cancellationToken))
        {
            sawResult = true;

            if (!string.Equals(
                reader.GetString(0),
                "ok",
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return sawResult;
    }

    private static async Task<IReadOnlyList<string>> ReadTableNamesAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT name
            FROM sqlite_master
            WHERE type = 'table'
              AND name NOT LIKE 'sqlite_%'
            ORDER BY name;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var names = new List<string>();

        while (await reader.ReadAsync(cancellationToken))
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    private static async Task<IReadOnlyList<string>> ReadAppliedMigrationsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT MigrationId
            FROM "__EFMigrationsHistory"
            ORDER BY MigrationId;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var migrations = new List<string>();

        while (await reader.ReadAsync(cancellationToken))
        {
            migrations.Add(reader.GetString(0));
        }

        return migrations;
    }

    private static bool IsTransientAvailabilityError(SqliteException exception)
    {
        return exception.SqliteErrorCode is 5 or 6 or 10 or 14;
    }

    private static MetadataDatabaseInspection BuildUnavailable(
        string? targetMigration,
        string reason)
    {
        return new MetadataDatabaseInspection(
            new MetadataDatabaseInspectionFacts(
                FileExists: true,
                PathOccupiedByNonFile: false,
                Accessible: false,
                IntegrityValid: null,
                HasMigrationHistoryTable: null,
                AppliedMigrationCount: null,
                UserTableCount: null,
                CurrentMigration: null,
                TargetMigration: targetMigration),
            [MetadataDatabaseState.Unavailable],
            MetadataDatabaseState.Unavailable,
            [reason]);
    }

    private static MetadataDatabaseInspection BuildInvalid(
        string? targetMigration,
        string reason)
    {
        return new MetadataDatabaseInspection(
            new MetadataDatabaseInspectionFacts(
                FileExists: true,
                PathOccupiedByNonFile: false,
                Accessible: true,
                IntegrityValid: false,
                HasMigrationHistoryTable: null,
                AppliedMigrationCount: null,
                UserTableCount: null,
                CurrentMigration: null,
                TargetMigration: targetMigration),
            [MetadataDatabaseState.Invalid],
            MetadataDatabaseState.Invalid,
            [reason]);
    }
}
