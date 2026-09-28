namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Reusable application use case for inspecting the metadata database
/// without performing any lifecycle mutation.
/// </summary>
public sealed class InspectMetadataDatabase(
    IMetadataDatabaseInspectionProvider provider)
{
    private readonly IMetadataDatabaseInspectionProvider _provider =
        provider ?? throw new ArgumentNullException(nameof(provider));

    public Task<MetadataDatabaseInspection> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        return _provider.InspectAsync(cancellationToken);
    }
}
