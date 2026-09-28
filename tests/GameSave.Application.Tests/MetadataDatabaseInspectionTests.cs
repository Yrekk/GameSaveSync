using GameSave.Application.MetadataDatabase;

namespace GameSave.Application.Tests;

public sealed class MetadataDatabaseInspectionTests
{
    [Fact]
    public void Constructor_RejectsSuggestedStateOutsideCandidates()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new MetadataDatabaseInspection(
                CreateFacts(),
                [MetadataDatabaseState.Uninitialized],
                MetadataDatabaseState.Invalid,
                ["reason"]));

        Assert.Equal("suggestedState", exception.ParamName);
    }

    [Fact]
    public void Constructor_DeduplicatesCandidateStates()
    {
        var inspection = new MetadataDatabaseInspection(
            CreateFacts(),
            [
                MetadataDatabaseState.Uninitialized,
                MetadataDatabaseState.Uninitialized,
                MetadataDatabaseState.Invalid,
            ],
            MetadataDatabaseState.Uninitialized,
            ["reason"]);

        Assert.Equal(
            [MetadataDatabaseState.Uninitialized, MetadataDatabaseState.Invalid],
            inspection.CandidateStates);
        Assert.True(inspection.RequiresAdministratorClassification);
    }

    private static MetadataDatabaseInspectionFacts CreateFacts()
    {
        return new MetadataDatabaseInspectionFacts(
            true,
            false,
            true,
            true,
            false,
            0,
            0,
            null,
            "target");
    }
}
