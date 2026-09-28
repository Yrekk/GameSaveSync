using GameSave.Application.MetadataDatabase;
using GameSave.Persistence.Database;

namespace GameSave.Persistence.Tests;

public sealed class XmlMetadataDatabaseClassificationStoreTests
{
    [Fact]
    public async Task MissingFile_IsValidEmptyHistory()
    {
        using var fixture = CreateFixture();
        var store = fixture.CreateStore();

        var result = await store.ReadAsync();

        Assert.Equal(
            MetadataDatabaseClassificationStoreStatus.Ready,
            result.Status);
        Assert.Empty(result.History);
        Assert.False(File.Exists(fixture.ControlPath));
    }

    [Fact]
    public async Task Append_WritesHumanReadableXmlAndRoundTrips()
    {
        using var fixture = CreateFixture();
        var store = fixture.CreateStore();
        var classification = CreateClassification(
            "Damien Ferrari",
            MetadataDatabaseState.Invalid,
            "Old development fixture; never adopt.");

        var status = await store.AppendAsync(classification);
        var read = await store.ReadAsync();
        var xml = await File.ReadAllTextAsync(fixture.ControlPath);

        Assert.Equal(
            MetadataDatabaseClassificationStoreStatus.Ready,
            status);
        var persisted = Assert.Single(read.History);
        Assert.Equal(classification.DecisionId, persisted.DecisionId);
        Assert.Equal("Damien Ferrari", persisted.ActorLabel);
        Assert.Equal(
            "Old development fixture; never adopt.",
            persisted.Rationale);

        Assert.Contains("<GameSaveControl version=\"1\">", xml);
        Assert.Contains("<Label>Damien Ferrari</Label>", xml);
        Assert.Contains(
            "<Label>GameSaveSync metadata database</Label>",
            xml);
        Assert.Contains(
            "<Rationale>Old development fixture; never adopt.</Rationale>",
            xml);

        Assert.Empty(
            Directory.GetFiles(
                Path.GetDirectoryName(fixture.ControlPath)!,
                "*.tmp"));
    }

    [Fact]
    public async Task Append_PreservesCompleteHistoryInOrder()
    {
        using var fixture = CreateFixture();
        var store = fixture.CreateStore();
        var first = CreateClassification(
            "Damien Ferrari",
            MetadataDatabaseState.Uninitialized);
        var second = CreateClassification(
            "Damien Ferrari",
            MetadataDatabaseState.Invalid,
            "Do not adopt this resource.");

        Assert.Equal(
            MetadataDatabaseClassificationStoreStatus.Ready,
            await store.AppendAsync(first));
        Assert.Equal(
            MetadataDatabaseClassificationStoreStatus.Ready,
            await store.AppendAsync(second));

        var read = await store.ReadAsync();

        Assert.Equal(
            [first.DecisionId, second.DecisionId],
            read.History.Select(item => item.DecisionId));
    }

    [Fact]
    public async Task MalformedXml_IsInvalidAndAppendDoesNotOverwriteIt()
    {
        using var fixture = CreateFixture();
        Directory.CreateDirectory(
            Path.GetDirectoryName(fixture.ControlPath)!);
        await File.WriteAllTextAsync(
            fixture.ControlPath,
            "<GameSaveControl><broken>");
        var original = await File.ReadAllTextAsync(fixture.ControlPath);
        var store = fixture.CreateStore();

        var read = await store.ReadAsync();
        var write = await store.AppendAsync(
            CreateClassification(
                "Damien Ferrari",
                MetadataDatabaseState.Uninitialized));

        Assert.Equal(
            MetadataDatabaseClassificationStoreStatus.Invalid,
            read.Status);
        Assert.Equal(
            MetadataDatabaseClassificationStoreStatus.Invalid,
            write);
        Assert.Equal(
            original,
            await File.ReadAllTextAsync(fixture.ControlPath));
    }

    private static AuthorizedMetadataDatabaseClassification CreateClassification(
        string actorLabel,
        MetadataDatabaseState selectedState,
        string? rationale = null)
    {
        return new AuthorizedMetadataDatabaseClassification(
            Guid.NewGuid(),
            "metadata-database:/srv/gamesave/data/gamesave-metadata.db",
            "GameSaveSync metadata database",
            "sha256:test",
            MetadataDatabaseClassificationPolicy.CurrentVersion,
            [
                MetadataDatabaseState.Uninitialized,
                MetadataDatabaseState.Invalid,
            ],
            MetadataDatabaseState.Uninitialized,
            selectedState,
            "admin:190992294",
            actorLabel,
            new DateTimeOffset(2026, 9, 28, 11, 0, 0, TimeSpan.Zero),
            rationale);
    }

    private static TestFixture CreateFixture()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "GameSaveSync.Tests",
            Guid.NewGuid().ToString("N"));

        return new TestFixture(root);
    }

    private sealed class TestFixture(string root) : IDisposable
    {
        public string ControlPath { get; } =
            Path.Combine(root, "control", "gamesave-control.xml");

        public XmlMetadataDatabaseClassificationStore CreateStore()
        {
            var settings =
                MetadataDatabaseControlStoreSettings.FromConfiguredPath(
                    Path.Combine("control", "gamesave-control.xml"),
                    root);

            return new XmlMetadataDatabaseClassificationStore(settings);
        }

        public void Dispose()
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
