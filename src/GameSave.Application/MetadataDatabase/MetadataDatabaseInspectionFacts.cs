namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Technical facts observed during a read-only metadata database inspection.
/// Administrative classification never rewrites these facts.
/// </summary>
public sealed record MetadataDatabaseInspectionFacts(
    bool FileExists,
    bool PathOccupiedByNonFile,
    bool Accessible,
    bool? IntegrityValid,
    bool? HasMigrationHistoryTable,
    int? AppliedMigrationCount,
    int? UserTableCount,
    string? CurrentMigration,
    string? TargetMigration);
