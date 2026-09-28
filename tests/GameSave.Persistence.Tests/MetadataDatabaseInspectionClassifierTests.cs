using GameSave.Application.MetadataDatabase;
using GameSave.Persistence.Database;

namespace GameSave.Persistence.Tests;

public sealed class MetadataDatabaseInspectionClassifierTests
{
    [Fact]
    public void NoAppliedMigrationAndNoUserTable_SuggestsUninitialized()
    {
        var inspection = Classify(0, ["001_Initial"], []);

        Assert.Equal(MetadataDatabaseState.Uninitialized, inspection.SuggestedState);
        Assert.Equal(
            [MetadataDatabaseState.Uninitialized, MetadataDatabaseState.Invalid],
            inspection.CandidateStates);
    }

    [Fact]
    public void NoAppliedMigrationWithUserTables_SuggestsInvalid()
    {
        var inspection = Classify(2, ["001_Initial"], []);

        Assert.Equal(MetadataDatabaseState.Invalid, inspection.SuggestedState);
        Assert.Equal(
            [MetadataDatabaseState.Uninitialized, MetadataDatabaseState.Invalid],
            inspection.CandidateStates);
    }

    [Fact]
    public void ValidOlderMigrationPrefix_SuggestsMigrationRequired()
    {
        var inspection = Classify(
            0,
            ["001_Initial", "002_Profile"],
            ["001_Initial"]);

        Assert.Equal(
            MetadataDatabaseState.MigrationRequired,
            inspection.SuggestedState);
        Assert.Contains(MetadataDatabaseState.Invalid, inspection.CandidateStates);
    }

    [Fact]
    public void AllKnownMigrationsApplied_SuggestsReady()
    {
        var inspection = Classify(
            1,
            ["001_Initial", "002_Profile"],
            ["001_Initial", "002_Profile"]);

        Assert.Equal(MetadataDatabaseState.Ready, inspection.SuggestedState);
        Assert.Equal(
            [MetadataDatabaseState.Ready, MetadataDatabaseState.Invalid],
            inspection.CandidateStates);
    }

    [Fact]
    public void UnknownAppliedMigration_SuggestsTooNew()
    {
        var inspection = Classify(
            0,
            ["001_Initial"],
            ["001_Initial", "002_Future"]);

        Assert.Equal(MetadataDatabaseState.TooNew, inspection.SuggestedState);
        Assert.Equal(
            [MetadataDatabaseState.TooNew, MetadataDatabaseState.Invalid],
            inspection.CandidateStates);
    }

    [Fact]
    public void NonPrefixKnownMigrationHistory_IsInvalidOnly()
    {
        var inspection = Classify(
            0,
            ["001_Initial", "002_Profile"],
            ["002_Profile"]);

        Assert.Equal(MetadataDatabaseState.Invalid, inspection.SuggestedState);
        Assert.Equal([MetadataDatabaseState.Invalid], inspection.CandidateStates);
    }

    private static MetadataDatabaseInspection Classify(
        int userTableCount,
        IReadOnlyList<string> knownMigrations,
        IReadOnlyList<string> appliedMigrations)
    {
        var facts = new MetadataDatabaseInspectionFacts(
            true,
            false,
            true,
            true,
            appliedMigrations.Count > 0,
            appliedMigrations.Count,
            userTableCount,
            appliedMigrations.LastOrDefault(),
            knownMigrations.LastOrDefault());

        return MetadataDatabaseInspectionClassifier.ClassifyAccessibleDatabase(
            facts,
            knownMigrations,
            appliedMigrations);
    }
}
