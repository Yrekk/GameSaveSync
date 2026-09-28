namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Infrastructure port for read-only metadata database discovery.
/// </summary>
public interface IMetadataDatabaseInspectionProvider
{
    Task<MetadataDatabaseInspection> InspectAsync(
        CancellationToken cancellationToken = default);
}
