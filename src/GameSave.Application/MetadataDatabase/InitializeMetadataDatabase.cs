namespace GameSave.Application.MetadataDatabase;

public sealed class InitializeMetadataDatabase(
    InspectAndResolveMetadataDatabase inspectAndResolve,
    IMetadataDatabaseLifecycleOperator lifecycle)
{
    private readonly InspectAndResolveMetadataDatabase _inspectAndResolve =
        inspectAndResolve
        ?? throw new ArgumentNullException(nameof(inspectAndResolve));

    private readonly IMetadataDatabaseLifecycleOperator _lifecycle =
        lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));

    public async Task<MetadataDatabaseAdministrationResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var current = await _inspectAndResolve.ExecuteAsync(cancellationToken);
        var state = current.Classification.EffectiveState;

        if (state is not (
            MetadataDatabaseState.Missing
            or MetadataDatabaseState.Uninitialized))
        {
            return new MetadataDatabaseAdministrationResult(
                MetadataDatabaseAdministrationStatus.NotAllowed,
                current.Inspection);
        }

        await _lifecycle.InitializeAsync(cancellationToken);

        var final = await _inspectAndResolve.ExecuteAsync(cancellationToken);

        return new MetadataDatabaseAdministrationResult(
            final.Classification.EffectiveState == MetadataDatabaseState.Ready
                ? MetadataDatabaseAdministrationStatus.Succeeded
                : MetadataDatabaseAdministrationStatus.FinalStateUnexpected,
            final.Inspection);
    }
}
