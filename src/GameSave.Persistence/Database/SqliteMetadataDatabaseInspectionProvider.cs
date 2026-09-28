using GameSave.Application.Inspection;
using GameSave.Application.MetadataDatabase;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Database;

/// <summary>
/// SQLite/EF implementation of read-only metadata database inspection.
/// </summary>
internal sealed class SqliteMetadataDatabaseInspectionProvider
    : IMetadataDatabaseInspectionProvider
{
    private const string EfMigrationHistoryTable = "__EFMigrationsHistory";

    private readonly MetadataDatabaseSettings _settings;
    private readonly string _connectionString;

    public SqliteMetadataDatabaseInspectionProvider(
        MetadataDatabaseSettings settings)
        : this(
            settings,
            MetadataDatabaseConnectionStrings.ForOperationalUse(settings))
    {
    }

    internal SqliteMetadataDatabaseInspectionProvider(
        MetadataDatabaseSettings settings,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "Inspection connection string must not be empty.",
                nameof(connectionString));
        }

        _settings = settings;
        _connectionString = connectionString;
    }

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
                    new InspectionFinding(
                        occupiedByNonFile
                            ? MetadataDatabaseFindingCodes.ResourcePathNotFile
                            : MetadataDatabaseFindingCodes.ResourceMissing),
                ]);
        }

        await using var connection = new SqliteConnection(_connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
        }
        catch (SqliteException exception) when (IsTransientAvailabilityError(exception))
        {
            return BuildUnavailable(
                targetMigration,
                providerErrorCode: exception.SqliteErrorCode);
        }
        catch (SqliteException exception)
        {
            return BuildInvalid(
                targetMigration,
                integrityValid: false,
                MetadataDatabaseFindingCodes.IntegrityFailed,
                providerErrorCode: exception.SqliteErrorCode);
        }
        catch (IOException)
        {
            return BuildUnavailable(targetMigration);
        }
        catch (UnauthorizedAccessException)
        {
            return BuildUnavailable(targetMigration);
        }

        try
        {
            if (!await CheckIntegrityAsync(connection, cancellationToken))
            {
                return BuildInvalid(
                    targetMigration,
                    integrityValid: false,
                    MetadataDatabaseFindingCodes.IntegrityFailed);
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
                providerErrorCode: exception.SqliteErrorCode);
        }
        catch (SqliteException exception)
        {
            return BuildInvalid(
                targetMigration,
                integrityValid: true,
                MetadataDatabaseFindingCodes.SchemaReadFailed,
                providerErrorCode: exception.SqliteErrorCode);
        }
    }

    private IReadOnlyList<string> GetKnownMigrations()
    {
        var options = new DbContextOptionsBuilder<GameSaveDbContext>()
            .UseSqlite(_connectionString)
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
        int? providerErrorCode = null)
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
            [
                new InspectionFinding(
                    MetadataDatabaseFindingCodes.ResourceUnavailable,
                    providerErrorCode is null
                        ? null
                        : new Dictionary<string, object?>
                        {
                            ["provider_error_code"] = providerErrorCode.Value,
                        }),
            ]);
    }

    private static MetadataDatabaseInspection BuildInvalid(
        string? targetMigration,
        bool integrityValid,
        string findingCode,
        int? providerErrorCode = null)
    {
        return new MetadataDatabaseInspection(
            new MetadataDatabaseInspectionFacts(
                FileExists: true,
                PathOccupiedByNonFile: false,
                Accessible: true,
                IntegrityValid: integrityValid,
                HasMigrationHistoryTable: null,
                AppliedMigrationCount: null,
                UserTableCount: null,
                CurrentMigration: null,
                TargetMigration: targetMigration),
            [MetadataDatabaseState.Invalid],
            MetadataDatabaseState.Invalid,
            [
                new InspectionFinding(
                    findingCode,
                    providerErrorCode is null
                        ? null
                        : new Dictionary<string, object?>
                        {
                            ["provider_error_code"] = providerErrorCode.Value,
                        }),
            ]);
    }
}
