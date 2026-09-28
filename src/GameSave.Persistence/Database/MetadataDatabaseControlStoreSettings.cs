namespace GameSave.Persistence.Database;

/// <summary>
/// Validated location of the trusted metadata classification control-plane file.
/// </summary>
public sealed record MetadataDatabaseControlStoreSettings
{
    private MetadataDatabaseControlStoreSettings(string controlFilePath)
    {
        ControlFilePath = controlFilePath;
    }

    public string ControlFilePath { get; }

    public static MetadataDatabaseControlStoreSettings FromConfiguredPath(
        string? configuredPath,
        string basePath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new ArgumentException(
                "Metadata control-store path must be configured.",
                nameof(configuredPath));
        }

        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new ArgumentException(
                "Metadata control-store base path must not be empty.",
                nameof(basePath));
        }

        var fullBasePath = Path.GetFullPath(basePath);
        var fullPath = Path.IsPathFullyQualified(configuredPath)
            ? Path.GetFullPath(configuredPath)
            : Path.GetFullPath(configuredPath, fullBasePath);

        return new MetadataDatabaseControlStoreSettings(fullPath);
    }
}
