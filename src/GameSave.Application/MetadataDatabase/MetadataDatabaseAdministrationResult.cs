namespace GameSave.Application.MetadataDatabase;

public sealed record MetadataDatabaseAdministrationResult(
    MetadataDatabaseAdministrationStatus Status,
    MetadataDatabaseInspection Inspection,
    MetadataDatabaseSnapshotInfo? Snapshot = null);
