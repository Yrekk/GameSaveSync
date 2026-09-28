using GameSave.Application.Inspection;
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
                [new InspectionFinding("database.test")]));

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
            [new InspectionFinding("database.test")]);

        Assert.Equal(
            [MetadataDatabaseState.Uninitialized, MetadataDatabaseState.Invalid],
            inspection.CandidateStates);
        Assert.True(inspection.RequiresAdministratorClassification);
    }

    [Fact]
    public void Finding_RejectsBlankCode()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new InspectionFinding(" "));

        Assert.Equal("code", exception.ParamName);
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
