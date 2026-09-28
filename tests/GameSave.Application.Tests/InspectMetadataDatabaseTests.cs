using GameSave.Application.Inspection;
using GameSave.Application.MetadataDatabase;

namespace GameSave.Application.Tests;

public sealed class InspectMetadataDatabaseTests
{
    [Fact]
    public async Task ExecuteAsync_DelegatesInspectionToProvider()
    {
        var expected = new MetadataDatabaseInspection(
            new MetadataDatabaseInspectionFacts(
                false,
                false,
                false,
                null,
                null,
                null,
                null,
                null,
                "target"),
            [MetadataDatabaseState.Missing],
            MetadataDatabaseState.Missing,
            [new InspectionFinding(MetadataDatabaseFindingCodes.ResourceMissing)]);

        var provider = new StubProvider(expected);
        var useCase = new InspectMetadataDatabase(provider);

        var actual = await useCase.ExecuteAsync();

        Assert.Same(expected, actual);
        Assert.Equal(1, provider.CallCount);
    }

    private sealed class StubProvider(
        MetadataDatabaseInspection inspection)
        : IMetadataDatabaseInspectionProvider
    {
        public int CallCount { get; private set; }

        public Task<MetadataDatabaseInspection> InspectAsync(
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(inspection);
        }
    }
}
