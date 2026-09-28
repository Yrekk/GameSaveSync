using System.Globalization;
using System.Text.Json;
using GameSave.Application.MetadataDatabase;
using GameSave.Persistence.Profiles;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Database;

internal sealed class SqliteMetadataDatabaseSnapshotStore(
    MetadataDatabaseSettings databaseSettings,
    MetadataDatabaseSnapshotSettings snapshotSettings,
    TimeProvider? timeProvider = null)
    : IMetadataDatabaseSnapshotStore
{
    private const int RollingRetention = 2;

    private readonly MetadataDatabaseSettings _databaseSettings =
        databaseSettings ?? throw new ArgumentNullException(nameof(databaseSettings));

    private readonly MetadataDatabaseSnapshotSettings _snapshotSettings =
        snapshotSettings ?? throw new ArgumentNullException(nameof(snapshotSettings));

    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<MetadataDatabaseSnapshotInfo> CreateAsync(
        MetadataDatabaseSnapshotKind kind,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_snapshotSettings.DirectoryPath);

        var now = _timeProvider.GetUtcNow();
        var id = BuildId(kind, now);
        var path = SnapshotPath(id);

        await using var source = new SqliteConnection(
            MetadataDatabaseConnectionStrings.ForOperationalUse(
                _databaseSettings));
        await using var destination = new SqliteConnection(
            new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadWriteCreate,
                ForeignKeys = true,
            }.ToString());

        await source.OpenAsync(cancellationToken);
        await destination.OpenAsync(cancellationToken);

        source.BackupDatabase(destination);

        var valid = await ValidateSnapshotAsync(path, cancellationToken);

        if (kind == MetadataDatabaseSnapshotKind.Rolling && valid)
        {
            RotateRollingSnapshots();
        }

        return new MetadataDatabaseSnapshotInfo(id, kind, now, valid);
    }

    public async Task<IReadOnlyList<MetadataDatabaseSnapshotInfo>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_snapshotSettings.DirectoryPath))
        {
            return [];
        }

        var results = new List<MetadataDatabaseSnapshotInfo>();

        foreach (var path in Directory.EnumerateFiles(
            _snapshotSettings.DirectoryPath,
            "*.db",
            SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryParseSnapshot(
                    Path.GetFileNameWithoutExtension(path),
                    out var id,
                    out var kind,
                    out var createdAt))
            {
                continue;
            }

            results.Add(
                new MetadataDatabaseSnapshotInfo(
                    id,
                    kind,
                    createdAt,
                    await ValidateSnapshotAsync(path, cancellationToken)));
        }

        return results
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToArray();
    }

    public async Task<bool> RestoreAsync(
        string snapshotId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotId);

        if (!IsSafeSnapshotId(snapshotId))
        {
            return false;
        }

        var sourcePath = SnapshotPath(snapshotId);

        if (!File.Exists(sourcePath)
            || !await ValidateSnapshotAsync(sourcePath, cancellationToken))
        {
            return false;
        }

        var activePath = _databaseSettings.DatabasePath;
        var activeDirectory = Path.GetDirectoryName(activePath);

        if (string.IsNullOrWhiteSpace(activeDirectory))
        {
            return false;
        }

        Directory.CreateDirectory(activeDirectory);
        Directory.CreateDirectory(_snapshotSettings.DirectoryPath);

        var tempPath = $"{activePath}.{Guid.NewGuid():N}.restore.tmp";
        var quarantinePath = Path.Combine(
            _snapshotSettings.DirectoryPath,
            $"failed-active-{_timeProvider.GetUtcNow():yyyyMMddTHHmmssfffffffZ}-{Guid.NewGuid():N}.db");

        File.Copy(sourcePath, tempPath, overwrite: false);

        if (!await ValidateSnapshotAsync(tempPath, cancellationToken))
        {
            File.Delete(tempPath);
            return false;
        }

        var movedActive = false;

        try
        {
            // Clear only the pool for the active metadata connection string.
            // A process-global ClearAllPools would interfere with unrelated
            // SQLite users and was explicitly rejected in the persistence foundation.
            using (var poolMarker = new SqliteConnection(
                MetadataDatabaseConnectionStrings.ForOperationalUse(
                    _databaseSettings)))
            {
                SqliteConnection.ClearPool(poolMarker);
            }

            if (File.Exists(activePath))
            {
                File.Move(activePath, quarantinePath);
                movedActive = true;
            }

            File.Move(tempPath, activePath);
            return true;
        }
        catch (IOException)
        {
            if (movedActive
                && !File.Exists(activePath)
                && File.Exists(quarantinePath))
            {
                File.Move(quarantinePath, activePath);
            }

            return false;
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private async Task<bool> ValidateSnapshotAsync(
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadOnly,
                ForeignKeys = true,
                Pooling = false,
            }.ToString();

            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            if (!await QuickCheckAsync(connection, cancellationToken))
            {
                return false;
            }

            var tables = await ReadTableNamesAsync(connection, cancellationToken);

            if (!tables.Contains("__EFMigrationsHistory", StringComparer.Ordinal))
            {
                return false;
            }

            var applied = await ReadAppliedMigrationsAsync(
                connection,
                cancellationToken);
            var known = GetKnownMigrations(connectionString);

            if (applied.Count == 0
                || applied.Count > known.Count
                || !applied.SequenceEqual(
                    known.Take(applied.Count),
                    StringComparer.Ordinal))
            {
                return false;
            }

            if (tables.Contains("GameProfiles", StringComparer.Ordinal))
            {
                await ValidateProfilesAsync(connection, cancellationToken);
            }

            return true;
        }
        catch (SqliteException)
        {
            return false;
        }
        catch (InvalidDataException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private IReadOnlyList<string> GetKnownMigrations(string connectionString)
    {
        var options = new DbContextOptionsBuilder<GameSaveDbContext>()
            .UseSqlite(connectionString)
            .Options;

        using var context = new GameSaveDbContext(options);
        return context.Database.GetMigrations().ToArray();
    }

    private static async Task ValidateProfilesAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT ProfileId, DisplayName, PayloadJson
            FROM GameProfiles
            ORDER BY ProfileId;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var record = new GameProfileRecord
            {
                ProfileId = reader.GetString(0),
                DisplayName = reader.GetString(1),
                PayloadJson = reader.GetString(2),
            };

            var document = JsonSerializer.Deserialize<GameProfileDocument>(
                record.PayloadJson,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                })
                ?? throw new InvalidDataException(
                    $"Snapshot profile '{record.ProfileId}' payload is empty.");

            if (!string.Equals(
                    document.Id,
                    record.ProfileId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    document.DisplayName,
                    record.DisplayName,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"Snapshot profile '{record.ProfileId}' metadata mismatch.");
            }

            _ = document.ToDomain();
        }
    }

    private static async Task<bool> QuickCheckAsync(
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
        var values = new List<string>();

        while (await reader.ReadAsync(cancellationToken))
        {
            values.Add(reader.GetString(0));
        }

        return values;
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
        var values = new List<string>();

        while (await reader.ReadAsync(cancellationToken))
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }

    private void RotateRollingSnapshots()
    {
        var rolling = Directory.EnumerateFiles(
                _snapshotSettings.DirectoryPath,
                "*-rolling-*.db",
                SearchOption.TopDirectoryOnly)
            .OrderByDescending(path => Path.GetFileName(path), StringComparer.Ordinal)
            .Skip(RollingRetention)
            .ToArray();

        foreach (var path in rolling)
        {
            File.Delete(path);
        }
    }

    private string SnapshotPath(string id)
    {
        return Path.Combine(_snapshotSettings.DirectoryPath, $"{id}.db");
    }

    private static string BuildId(
        MetadataDatabaseSnapshotKind kind,
        DateTimeOffset createdAt)
    {
        var kindText = kind switch
        {
            MetadataDatabaseSnapshotKind.Rolling => "rolling",
            MetadataDatabaseSnapshotKind.PreMigration => "pre-migration",
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        return $"{createdAt:yyyyMMddTHHmmssfffffffZ}-{kindText}-{Guid.NewGuid():N}";
    }

    private static bool TryParseSnapshot(
        string value,
        out string id,
        out MetadataDatabaseSnapshotKind kind,
        out DateTimeOffset createdAt)
    {
        id = value;
        kind = default;
        createdAt = default;

        var parts = value.Split('-', 3);

        if (parts.Length < 3
            || !DateTimeOffset.TryParseExact(
                parts[0],
                "yyyyMMddTHHmmssfffffffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal
                    | DateTimeStyles.AdjustToUniversal,
                out createdAt))
        {
            return false;
        }

        var tail = value[(parts[0].Length + 1)..];

        if (tail.StartsWith("rolling-", StringComparison.Ordinal))
        {
            kind = MetadataDatabaseSnapshotKind.Rolling;
        }
        else if (tail.StartsWith("pre-migration-", StringComparison.Ordinal))
        {
            kind = MetadataDatabaseSnapshotKind.PreMigration;
        }
        else
        {
            return false;
        }

        return IsSafeSnapshotId(value);
    }

    private static bool IsSafeSnapshotId(string value)
    {
        return value.Length <= 128
            && value.All(character =>
                char.IsAsciiLetterOrDigit(character)
                || character is '-' or 'T' or 'Z');
    }
}
