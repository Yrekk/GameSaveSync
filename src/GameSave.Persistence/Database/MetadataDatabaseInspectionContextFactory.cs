using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using GameSave.Application.MetadataDatabase;

namespace GameSave.Persistence.Database;

internal static class MetadataDatabaseInspectionContextFactory
{
    private const string ResourceLabel = "GameSaveSync metadata database";

    public static MetadataDatabaseInspectionContext Create(
        MetadataDatabaseSettings settings,
        MetadataDatabaseInspectionFacts facts,
        IReadOnlyList<string> knownMigrations,
        IReadOnlyList<string> appliedMigrations,
        IReadOnlyList<MetadataDatabaseSchemaEvidence> schemaEvidence,
        int? providerErrorCode = null)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(facts);
        ArgumentNullException.ThrowIfNull(knownMigrations);
        ArgumentNullException.ThrowIfNull(appliedMigrations);
        ArgumentNullException.ThrowIfNull(schemaEvidence);

        var resourceIdentity =
            $"metadata-database:{Path.GetFullPath(settings.DatabasePath)}";

        var canonical = new StringBuilder();

        Add(canonical, "resource_identity", resourceIdentity);
        Add(
            canonical,
            "classification_policy_version",
            MetadataDatabaseClassificationPolicy.CurrentVersion.ToString(
                CultureInfo.InvariantCulture));

        Add(canonical, "file_exists", facts.FileExists);
        Add(canonical, "path_occupied_by_non_file", facts.PathOccupiedByNonFile);
        Add(canonical, "accessible", facts.Accessible);
        Add(canonical, "integrity_valid", facts.IntegrityValid);
        Add(
            canonical,
            "has_migration_history_table",
            facts.HasMigrationHistoryTable);
        Add(canonical, "applied_migration_count", facts.AppliedMigrationCount);
        Add(canonical, "user_table_count", facts.UserTableCount);
        Add(canonical, "current_migration", facts.CurrentMigration);
        Add(canonical, "target_migration", facts.TargetMigration);
        Add(canonical, "provider_error_code", providerErrorCode);

        AddList(canonical, "known_migration", knownMigrations);
        AddList(canonical, "applied_migration", appliedMigrations);

        foreach (var entry in schemaEvidence.OrderBy(entry => entry.Name, StringComparer.Ordinal))
        {
            Add(canonical, "schema_name", entry.Name);
            Add(canonical, "schema_sql", entry.Sql);
        }

        var revisionBytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(canonical.ToString()));
        var revision =
            $"sha256:{Convert.ToHexString(revisionBytes).ToLowerInvariant()}";

        return new MetadataDatabaseInspectionContext(
            resourceIdentity,
            ResourceLabel,
            revision,
            MetadataDatabaseClassificationPolicy.CurrentVersion);
    }

    private static void AddList(
        StringBuilder builder,
        string key,
        IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            Add(builder, key, value);
        }
    }

    private static void Add(
        StringBuilder builder,
        string key,
        object? value)
    {
        var text = value switch
        {
            null => "<null>",
            bool boolean => boolean ? "true" : "false",
            IFormattable formattable =>
                formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };

        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        builder
            .Append(key)
            .Append('=')
            .Append(encoded)
            .Append('\n');
    }
}
