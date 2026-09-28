namespace GameSave.Application.MetadataDatabase;

public sealed class MetadataDatabaseClassificationStoreReadResult
{
    public MetadataDatabaseClassificationStoreReadResult(
        MetadataDatabaseClassificationStoreStatus status,
        IEnumerable<AuthorizedMetadataDatabaseClassification>? history = null)
    {
        if (status != MetadataDatabaseClassificationStoreStatus.Ready
            && history is not null
            && history.Any())
        {
            throw new ArgumentException(
                "Unavailable or invalid stores must not expose trusted history.",
                nameof(history));
        }

        Status = status;
        History = Array.AsReadOnly((history ?? []).ToArray());
    }

    public MetadataDatabaseClassificationStoreStatus Status { get; }

    public IReadOnlyList<AuthorizedMetadataDatabaseClassification> History { get; }
}
