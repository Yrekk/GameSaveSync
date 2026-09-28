namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Low-level metadata schema mutations. Application use cases decide when
/// these operations are allowed and always re-inspect afterwards.
/// </summary>
public interface IMetadataDatabaseLifecycleOperator
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task ApplyPendingMigrationsAsync(
        CancellationToken cancellationToken = default);
}
