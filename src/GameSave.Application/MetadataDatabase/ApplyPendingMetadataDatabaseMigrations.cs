namespace GameSave.Application.MetadataDatabase;

public sealed class ApplyPendingMetadataDatabaseMigrations(
    InspectAndResolveMetadataDatabase inspectAndResolve,
    IMetadataDatabaseLifecycleOperator lifecycle,
    IMetadataDatabaseSnapshotStore snapshots)
{
    private readonly InspectAndResolveMetadataDatabase _inspectAndResolve =
        inspectAndResolve
        ?? throw new ArgumentNullException(nameof(inspectAndResolve));

    private readonly IMetadataDatabaseLifecycleOperator _lifecycle =
        lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));

    private readonly IMetadataDatabaseSnapshotStore _snapshots =
        snapshots ?? throw new ArgumentNullException(nameof(snapshots));

    public async Task<MetadataDatabaseAdministrationResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var current = await _inspectAndResolve.ExecuteAsync(cancellationToken);

        if (current.Classification.EffectiveState
            != MetadataDatabaseState.MigrationRequired)
        {
            return new MetadataDatabaseAdministrationResult(
                MetadataDatabaseAdministrationStatus.NotAllowed,
                current.Inspection);
        }

        // Migration is never attempted without first producing a validated
        // SQLite-safe rollback source.
        var snapshot = await _snapshots.CreateAsync(
            MetadataDatabaseSnapshotKind.PreMigration,
            cancellationToken);

        if (!snapshot.IsValid)
        {
            return new MetadataDatabaseAdministrationResult(
                MetadataDatabaseAdministrationStatus.SnapshotInvalid,
                current.Inspection,
                snapshot);
        }

        await _lifecycle.ApplyPendingMigrationsAsync(cancellationToken);

        var final = await _inspectAndResolve.ExecuteAsync(cancellationToken);

        return new MetadataDatabaseAdministrationResult(
            final.Classification.EffectiveState == MetadataDatabaseState.Ready
                ? MetadataDatabaseAdministrationStatus.Succeeded
                : MetadataDatabaseAdministrationStatus.FinalStateUnexpected,
            final.Inspection,
            snapshot);
    }
}
