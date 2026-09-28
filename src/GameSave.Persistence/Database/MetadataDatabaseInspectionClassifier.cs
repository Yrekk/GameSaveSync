using GameSave.Application.MetadataDatabase;

namespace GameSave.Persistence.Database;

internal static class MetadataDatabaseInspectionClassifier
{
    public static MetadataDatabaseInspection ClassifyAccessibleDatabase(
        MetadataDatabaseInspectionFacts facts,
        IReadOnlyList<string> knownMigrations,
        IReadOnlyList<string> appliedMigrations)
    {
        ArgumentNullException.ThrowIfNull(facts);
        ArgumentNullException.ThrowIfNull(knownMigrations);
        ArgumentNullException.ThrowIfNull(appliedMigrations);

        if (!facts.FileExists || !facts.Accessible || facts.IntegrityValid is not true)
        {
            throw new ArgumentException(
                "Accessible-database classification requires an existing, accessible, integrity-valid database.",
                nameof(facts));
        }

        if (appliedMigrations.Count == 0)
        {
            var hasUserTables = facts.UserTableCount.GetValueOrDefault() > 0;

            return new MetadataDatabaseInspection(
                facts,
                [MetadataDatabaseState.Uninitialized, MetadataDatabaseState.Invalid],
                hasUserTables
                    ? MetadataDatabaseState.Invalid
                    : MetadataDatabaseState.Uninitialized,
                hasUserTables
                    ?
                    [
                        "No applied GameSaveSync migration was found.",
                        "Non-system user tables are present, so the database appears foreign or intentionally unmanaged.",
                    ]
                    :
                    [
                        "No applied GameSaveSync migration was found.",
                        "No non-system user table was found, so the database appears uninitialized.",
                    ]);
        }

        var unknownApplied = appliedMigrations
            .Where(migration => !knownMigrations.Contains(migration, StringComparer.Ordinal))
            .ToArray();

        if (unknownApplied.Length > 0)
        {
            return new MetadataDatabaseInspection(
                facts,
                [MetadataDatabaseState.TooNew, MetadataDatabaseState.Invalid],
                MetadataDatabaseState.TooNew,
                [
                    "The database contains migration identifiers that are unknown to this GameSaveSync binary.",
                    $"Unknown migration count: {unknownApplied.Length}.",
                ]);
        }

        var expectedPrefix = knownMigrations.Take(appliedMigrations.Count).ToArray();

        if (!appliedMigrations.SequenceEqual(expectedPrefix, StringComparer.Ordinal))
        {
            return new MetadataDatabaseInspection(
                facts,
                [MetadataDatabaseState.Invalid],
                MetadataDatabaseState.Invalid,
                [
                    "The applied GameSaveSync migration history is not a valid prefix of the migration sequence known by this binary.",
                ]);
        }

        if (appliedMigrations.Count < knownMigrations.Count)
        {
            return new MetadataDatabaseInspection(
                facts,
                [MetadataDatabaseState.MigrationRequired, MetadataDatabaseState.Invalid],
                MetadataDatabaseState.MigrationRequired,
                [
                    "The database has a valid older GameSaveSync migration history.",
                    $"{knownMigrations.Count - appliedMigrations.Count} known migration(s) are pending.",
                ]);
        }

        if (appliedMigrations.Count == knownMigrations.Count)
        {
            return new MetadataDatabaseInspection(
                facts,
                [MetadataDatabaseState.Ready, MetadataDatabaseState.Invalid],
                MetadataDatabaseState.Ready,
                [
                    "All GameSaveSync migrations known by this binary are applied in order.",
                ]);
        }

        return new MetadataDatabaseInspection(
            facts,
            [MetadataDatabaseState.Invalid],
            MetadataDatabaseState.Invalid,
            [
                "The migration history cannot be reconciled with the migration sequence known by this binary.",
            ]);
    }
}
