namespace GameSave.Application.MetadataDatabase;

public sealed class InspectAndResolveMetadataDatabase(
    InspectMetadataDatabase inspect,
    ResolveMetadataDatabaseClassification resolve)
{
    private readonly InspectMetadataDatabase _inspect =
        inspect ?? throw new ArgumentNullException(nameof(inspect));

    private readonly ResolveMetadataDatabaseClassification _resolve =
        resolve ?? throw new ArgumentNullException(nameof(resolve));

    public async Task<MetadataDatabaseResolvedInspection> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var inspection = await _inspect.ExecuteAsync(cancellationToken);
        var classification = await _resolve.ExecuteAsync(
            inspection,
            cancellationToken);

        return new MetadataDatabaseResolvedInspection(
            inspection,
            classification);
    }
}
