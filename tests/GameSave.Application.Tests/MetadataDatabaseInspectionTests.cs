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
                CreateContext(),
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
            CreateContext(),
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

    [Fact]
    public void Finding_AcceptsAndCopiesPrimitiveCollections()
    {
        var identifiers = new List<string> { "001_Initial", "002_Profile" };
        var finding = new InspectionFinding(
            "database.test",
            new Dictionary<string, object?>
            {
                ["identifiers"] = identifiers,
            });

        identifiers[0] = "mutated";

        var stored = Assert.IsAssignableFrom<IReadOnlyList<object?>>(
            finding.Details["identifiers"]);
        Assert.Equal(
            new object?[] { "001_Initial", "002_Profile" },
            stored);
    }

    [Fact]
    public void Finding_RejectsNestedOrUnsupportedCollectionValues()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new InspectionFinding(
                "database.test",
                new Dictionary<string, object?>
                {
                    ["invalid"] = new object?[] { new[] { "nested" } },
                }));

        Assert.Equal("details", exception.ParamName);
    }

    private static MetadataDatabaseInspectionContext CreateContext()
    {
        return new MetadataDatabaseInspectionContext(
            "metadata:test",
            "Test metadata database",
            "revision:test",
            MetadataDatabaseClassificationPolicy.CurrentVersion);
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
