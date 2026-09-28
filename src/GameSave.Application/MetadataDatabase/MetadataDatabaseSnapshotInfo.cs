namespace GameSave.Application.MetadataDatabase;

public sealed record MetadataDatabaseSnapshotInfo(
    string Id,
    MetadataDatabaseSnapshotKind Kind,
    DateTimeOffset CreatedAtUtc,
    bool IsValid);
