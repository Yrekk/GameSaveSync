namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// SQLite-safe metadata snapshot infrastructure.
/// Restore always targets an explicit snapshot id; no implementation may
/// silently choose the newest candidate.
/// </summary>
public interface IMetadataDatabaseSnapshotStore
{
    Task<MetadataDatabaseSnapshotInfo> CreateAsync(
        MetadataDatabaseSnapshotKind kind,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MetadataDatabaseSnapshotInfo>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<bool> RestoreAsync(
        string snapshotId,
        CancellationToken cancellationToken = default);
}
