using GameSave.Application.Storage;

namespace GameSave.Storage.Local;

internal sealed class LocalGameSaveArtifactStorage(
    LocalSaveArtifactStorageSettings settings)
    : IGameSaveArtifactStorage
{
    private readonly LocalSaveArtifactStorageSettings _settings =
        settings ?? throw new ArgumentNullException(nameof(settings));

    public async Task WriteAsync(
        SaveArtifactKey key,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(content);

        var path = ResolvePath(key);
        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException(
                "Artifact path did not resolve to a directory.");

        Directory.CreateDirectory(directory);

        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";

        try
        {
            await using (var output = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await content.CopyToAsync(output, cancellationToken);
                await output.FlushAsync(cancellationToken);
                output.Flush(flushToDisk: true);
            }

            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    public Task<Stream?> OpenReadAsync(
        SaveArtifactKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        cancellationToken.ThrowIfCancellationRequested();

        var path = ResolvePath(key);

        Stream? stream = File.Exists(path)
            ? new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan)
            : null;

        return Task.FromResult(stream);
    }

    public Task<bool> ExistsAsync(
        SaveArtifactKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(ResolvePath(key)));
    }

    public Task<SaveStorageStatus> CheckStatusAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var ready = Directory.Exists(_settings.RootPath)
            && !File.Exists(_settings.RootPath);

        return Task.FromResult(
            ready
                ? SaveStorageStatus.Ready
                : SaveStorageStatus.Unavailable);
    }

    private string ResolvePath(SaveArtifactKey key)
    {
        var root = Path.GetFullPath(_settings.RootPath);

        var path = Path.GetFullPath(
            Path.Combine(
                root,
                key.ProfileId.Value,
                key.ArtifactId.Value.ToString("N"),
                key.DataRootId.Value,
                key.RelativePath.Replace('/', Path.DirectorySeparatorChar)));

        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!path.StartsWith(
                rootWithSeparator,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Resolved artifact path escaped the configured storage root.");
        }

        return path;
    }
}
