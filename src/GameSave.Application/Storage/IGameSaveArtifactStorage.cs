namespace GameSave.Application.Storage;

/// <summary>
/// GameSaveSync-specific save artifact storage port.
/// Physical filesystem/NAS layout remains an infrastructure concern.
/// </summary>
public interface IGameSaveArtifactStorage
{
    Task WriteAsync(
        SaveArtifactKey key,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        SaveArtifactKey key,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        SaveArtifactKey key,
        CancellationToken cancellationToken = default);

    Task<SaveStorageStatus> CheckStatusAsync(
        CancellationToken cancellationToken = default);
}
