using System.Text;
using GameSave.Application.Storage;
using GameSave.Core.Profiles;
using GameSave.Storage.Local;

namespace GameSave.Storage.Tests;

public sealed class LocalGameSaveArtifactStorageTests
{
    [Fact]
    public async Task WriteAndRead_RoundTripsArtifact()
    {
        using var fixture = new StorageFixture();
        var storage = fixture.CreateStorage();
        var key = CreateKey("slot/save.bin");
        await using var content = new MemoryStream(
            Encoding.UTF8.GetBytes("save-data"));

        await storage.WriteAsync(key, content);

        Assert.True(await storage.ExistsAsync(key));
        await using var read = await storage.OpenReadAsync(key);
        Assert.NotNull(read);

        using var reader = new StreamReader(read, Encoding.UTF8);
        Assert.Equal("save-data", await reader.ReadToEndAsync());
    }

    [Theory]
    [InlineData("../escape.bin")]
    [InlineData("slot/../../escape.bin")]
    [InlineData("/absolute.bin")]
    public void ArtifactKey_RejectsUnsafePaths(string relativePath)
    {
        Assert.Throws<ArgumentException>(
            () => CreateKey(relativePath));
    }

    [Fact]
    public async Task CheckStatus_ReportsConfiguredExistingRootReady()
    {
        using var fixture = new StorageFixture();
        Directory.CreateDirectory(fixture.RootPath);

        var status = await fixture.CreateStorage().CheckStatusAsync();

        Assert.Equal(SaveStorageStatus.Ready, status);
    }

    private static SaveArtifactKey CreateKey(string relativePath)
    {
        return new SaveArtifactKey(
            new ProfileId("game"),
            new SaveArtifactId(Guid.CreateVersion7()),
            new DataRootId("main"),
            relativePath);
    }

    private sealed class StorageFixture : IDisposable
    {
        private readonly string _basePath = Path.Combine(
            Path.GetTempPath(),
            "GameSaveSync.Tests",
            Guid.NewGuid().ToString("N"));

        public string RootPath => Path.Combine(_basePath, "storage");

        public LocalGameSaveArtifactStorage CreateStorage()
        {
            return new LocalGameSaveArtifactStorage(
                LocalSaveArtifactStorageSettings.FromConfiguredPath(
                    "storage",
                    _basePath));
        }

        public void Dispose()
        {
            if (Directory.Exists(_basePath))
            {
                Directory.Delete(_basePath, recursive: true);
            }
        }
    }
}
