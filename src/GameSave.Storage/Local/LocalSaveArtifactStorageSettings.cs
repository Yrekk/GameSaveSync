namespace GameSave.Storage.Local;

public sealed record LocalSaveArtifactStorageSettings
{
    private LocalSaveArtifactStorageSettings(string rootPath)
    {
        RootPath = rootPath;
    }

    public string RootPath { get; }

    public static LocalSaveArtifactStorageSettings FromConfiguredPath(
        string? configuredPath,
        string basePath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new ArgumentException(
                "Save storage root path must be configured.",
                nameof(configuredPath));
        }

        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new ArgumentException(
                "Save storage base path must not be empty.",
                nameof(basePath));
        }

        var fullBasePath = Path.GetFullPath(basePath);
        var fullRoot = Path.IsPathFullyQualified(configuredPath)
            ? Path.GetFullPath(configuredPath)
            : Path.GetFullPath(configuredPath, fullBasePath);

        return new LocalSaveArtifactStorageSettings(fullRoot);
    }
}
