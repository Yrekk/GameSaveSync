using GameSave.Persistence.Database;

namespace GameSave.Persistence.Tests;

public sealed class MetadataDatabaseSettingsTests
{
    [Fact]
    public void FromConfiguredPath_RejectsMissingPath()
    {
        var basePath = Path.GetTempPath();

        var exception = Assert.Throws<ArgumentException>(
            () => MetadataDatabaseSettings.FromConfiguredPath(null, basePath));

        Assert.Contains("must be configured", exception.Message);
    }

    [Fact]
    public void FromConfiguredPath_ResolvesRelativePathAgainstExplicitBasePath()
    {
        var basePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var settings = MetadataDatabaseSettings.FromConfiguredPath(
            Path.Combine("data", "metadata.db"),
            basePath);

        Assert.Equal(
            Path.GetFullPath(Path.Combine(basePath, "data", "metadata.db")),
            settings.DatabasePath);
    }

    [Fact]
    public void FromConfiguredPath_PreservesAbsolutePath()
    {
        var databasePath = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "metadata.db"));

        var settings = MetadataDatabaseSettings.FromConfiguredPath(
            databasePath,
            Path.GetTempPath());

        Assert.Equal(databasePath, settings.DatabasePath);
    }
}
