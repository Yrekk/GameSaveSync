using GameSave.Application.Inspection;
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
            var findings = new List<InspectionFinding>
            {
                new(MetadataDatabaseFindingCodes.SchemaUninitialized),
                new(MetadataDatabaseFindingCodes.ApplicationHistoryAbsent),
            };

            findings.Add(
                hasUserTables
                    ? new InspectionFinding(
                        MetadataDatabaseFindingCodes.NonApplicationObjectsPresent,
                        new Dictionary<string, object?>
                        {
                            ["object_count"] = facts.UserTableCount.GetValueOrDefault(),
                        })
                    : new InspectionFinding(
                        MetadataDatabaseFindingCodes.UserObjectsAbsent));

            return new MetadataDatabaseInspection(
                facts,
                [MetadataDatabaseState.Uninitialized, MetadataDatabaseState.Invalid],
                hasUserTables
                    ? MetadataDatabaseState.Invalid
                    : MetadataDatabaseState.Uninitialized,
                findings);
        }

        var unknownApplied = appliedMigrations
            .Where(migration => !knownMigrations.Contains(
                migration,
                StringComparer.Ordinal))
            .ToArray();

        if (unknownApplied.Length > 0)
        {
            return new MetadataDatabaseInspection(
                facts,
                [MetadataDatabaseState.TooNew, MetadataDatabaseState.Invalid],
                MetadataDatabaseState.TooNew,
                [
                    new InspectionFinding(
                        MetadataDatabaseFindingCodes.SchemaNewerThanRuntime,
                        new Dictionary<string, object?>
                        {
                            ["unknown_count"] = unknownApplied.Length,
                        }),
                ]);
        }

        var expectedPrefix = knownMigrations
            .Take(appliedMigrations.Count)
            .ToArray();

        if (!appliedMigrations.SequenceEqual(
            expectedPrefix,
            StringComparer.Ordinal))
        {
            return new MetadataDatabaseInspection(
                facts,
                [MetadataDatabaseState.Invalid],
                MetadataDatabaseState.Invalid,
                [
                    new InspectionFinding(
                        MetadataDatabaseFindingCodes.SchemaHistoryInconsistent),
                ]);
        }

        if (appliedMigrations.Count < knownMigrations.Count)
        {
            return new MetadataDatabaseInspection(
                facts,
                [MetadataDatabaseState.MigrationRequired],
                MetadataDatabaseState.MigrationRequired,
                [
                    new InspectionFinding(
                        MetadataDatabaseFindingCodes.SchemaOutdated,
                        new Dictionary<string, object?>
                        {
                            ["pending_count"] =
                                knownMigrations.Count - appliedMigrations.Count,
                        }),
                ]);
        }

        if (appliedMigrations.Count == knownMigrations.Count)
        {
            return new MetadataDatabaseInspection(
                facts,
                [MetadataDatabaseState.Ready],
                MetadataDatabaseState.Ready,
                [
                    new InspectionFinding(
                        MetadataDatabaseFindingCodes.SchemaCurrent),
                ]);
        }

        return new MetadataDatabaseInspection(
            facts,
            [MetadataDatabaseState.Invalid],
            MetadataDatabaseState.Invalid,
            [
                new InspectionFinding(
                    MetadataDatabaseFindingCodes.SchemaHistoryInconsistent),
            ]);
    }
}
