namespace GameSave.Persistence.Database;

public sealed record MetadataDatabaseSnapshotSettings
{
    private MetadataDatabaseSnapshotSettings(string directoryPath)
    {
        DirectoryPath = directoryPath;
    }

    public string DirectoryPath { get; }

    public static MetadataDatabaseSnapshotSettings FromConfiguredPath(
        string? configuredPath,
        string basePath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new ArgumentException(
                "Metadata snapshot directory must be configured.",
                nameof(configuredPath));
        }

        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new ArgumentException(
                "Metadata snapshot base path must not be empty.",
                nameof(basePath));
        }

        var fullBase = Path.GetFullPath(basePath);
        var fullPath = Path.IsPathFullyQualified(configuredPath)
            ? Path.GetFullPath(configuredPath)
            : Path.GetFullPath(configuredPath, fullBase);

        return new MetadataDatabaseSnapshotSettings(fullPath);
    }
}
