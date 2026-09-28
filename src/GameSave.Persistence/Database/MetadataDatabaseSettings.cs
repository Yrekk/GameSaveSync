namespace GameSave.Persistence.Database;

/// <summary>
/// Validated location of the central metadata database.
/// </summary>
public sealed record MetadataDatabaseSettings
{
    private MetadataDatabaseSettings(string databasePath)
    {
        DatabasePath = databasePath;
    }

    public string DatabasePath { get; }

    public static MetadataDatabaseSettings FromConfiguredPath(
        string? configuredPath,
        string basePath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new ArgumentException(
                "Metadata database path must be configured.",
                nameof(configuredPath));
        }

        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new ArgumentException(
                "Metadata database base path must not be empty.",
                nameof(basePath));
        }

        var fullBasePath = Path.GetFullPath(basePath);
        var fullDatabasePath = Path.IsPathFullyQualified(configuredPath)
            ? Path.GetFullPath(configuredPath)
            : Path.GetFullPath(configuredPath, fullBasePath);

        return new MetadataDatabaseSettings(fullDatabasePath);
    }
}
