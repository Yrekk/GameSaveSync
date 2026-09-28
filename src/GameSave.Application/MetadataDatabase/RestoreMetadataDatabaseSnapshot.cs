namespace GameSave.Application.MetadataDatabase;

public sealed class RestoreMetadataDatabaseSnapshot(
    InspectAndResolveMetadataDatabase inspectAndResolve,
    IMetadataDatabaseSnapshotStore snapshots)
{
    private readonly InspectAndResolveMetadataDatabase _inspectAndResolve =
        inspectAndResolve
        ?? throw new ArgumentNullException(nameof(inspectAndResolve));

    private readonly IMetadataDatabaseSnapshotStore _snapshots =
        snapshots ?? throw new ArgumentNullException(nameof(snapshots));

    public async Task<MetadataDatabaseAdministrationResult> ExecuteAsync(
        string snapshotId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotId);

        var current = await _inspectAndResolve.ExecuteAsync(cancellationToken);
        var state = current.Classification.EffectiveState;

        if (state is MetadataDatabaseState.Ready
            or MetadataDatabaseState.MigrationRequired
            || state is null)
        {
            return new MetadataDatabaseAdministrationResult(
                MetadataDatabaseAdministrationStatus.NotAllowed,
                current.Inspection);
        }

        var candidates = await _snapshots.ListAsync(cancellationToken);
        var snapshot = candidates.SingleOrDefault(
            item => string.Equals(
                item.Id,
                snapshotId,
                StringComparison.Ordinal));

        if (snapshot is null || !snapshot.IsValid)
        {
            return new MetadataDatabaseAdministrationResult(
                MetadataDatabaseAdministrationStatus.SnapshotInvalid,
                current.Inspection,
                snapshot);
        }

        if (!await _snapshots.RestoreAsync(snapshotId, cancellationToken))
        {
            return new MetadataDatabaseAdministrationResult(
                MetadataDatabaseAdministrationStatus.RestoreFailed,
                current.Inspection,
                snapshot);
        }

        var final = await _inspectAndResolve.ExecuteAsync(cancellationToken);
        var finalState = final.Classification.EffectiveState;

        return new MetadataDatabaseAdministrationResult(
            finalState is MetadataDatabaseState.Ready
                or MetadataDatabaseState.MigrationRequired
                ? MetadataDatabaseAdministrationStatus.Succeeded
                : MetadataDatabaseAdministrationStatus.FinalStateUnexpected,
            final.Inspection,
            snapshot);
    }
}
