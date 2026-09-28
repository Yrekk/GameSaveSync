namespace GameSave.Application.MetadataDatabase;

public sealed class CreateRollingMetadataDatabaseSnapshot(
    InspectAndResolveMetadataDatabase inspectAndResolve,
    IMetadataDatabaseSnapshotStore snapshots)
{
    private readonly InspectAndResolveMetadataDatabase _inspectAndResolve =
        inspectAndResolve
        ?? throw new ArgumentNullException(nameof(inspectAndResolve));

    private readonly IMetadataDatabaseSnapshotStore _snapshots =
        snapshots ?? throw new ArgumentNullException(nameof(snapshots));

    public async Task<MetadataDatabaseAdministrationResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var current = await _inspectAndResolve.ExecuteAsync(cancellationToken);

        if (current.Classification.EffectiveState != MetadataDatabaseState.Ready)
        {
            return new MetadataDatabaseAdministrationResult(
                MetadataDatabaseAdministrationStatus.NotAllowed,
                current.Inspection);
        }

        var snapshot = await _snapshots.CreateAsync(
            MetadataDatabaseSnapshotKind.Rolling,
            cancellationToken);

        return new MetadataDatabaseAdministrationResult(
            snapshot.IsValid
                ? MetadataDatabaseAdministrationStatus.Succeeded
                : MetadataDatabaseAdministrationStatus.SnapshotInvalid,
            current.Inspection,
            snapshot);
    }
}
