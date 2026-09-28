namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Trusted control-plane persistence for durable authorized classifications.
/// </summary>
public interface IMetadataDatabaseClassificationStore
{
    Task<MetadataDatabaseClassificationStoreReadResult> ReadAsync(
        CancellationToken cancellationToken = default);

    Task<MetadataDatabaseClassificationStoreStatus> AppendAsync(
        AuthorizedMetadataDatabaseClassification classification,
        CancellationToken cancellationToken = default);
}
