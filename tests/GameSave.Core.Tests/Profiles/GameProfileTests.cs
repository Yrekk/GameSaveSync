using GameSave.Core.Profiles;

namespace GameSave.Core.Tests.Profiles;

public sealed class GameProfileTests
{
    [Fact]
    public void Constructor_AcceptsCompleteDisabledProfile()
    {
        var profile = CreateValidProfile(enabled: false);

        Assert.False(profile.Enabled);
        Assert.Equal("project-zomboid", profile.Id.Value);
        Assert.Single(profile.ProcessNames);
        Assert.Single(profile.DataRoots);
    }

    [Fact]
    public void Constructor_RejectsMissingProcessNames()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateValidProfile(processNames: []));
    }

    [Fact]
    public void Constructor_RejectsDuplicateProcessNamesCaseInsensitively()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateValidProfile(processNames: ["ProjectZomboid64", "projectzomboid64"]));
    }

    [Fact]
    public void Constructor_RejectsMissingDataRoots()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateValidProfile(dataRoots: []));
    }

    [Fact]
    public void Constructor_RejectsDuplicateDataRootIds()
    {
        var rootA = new GameDataRoot(new DataRootId("saves"), "C:\\A");
        var rootB = new GameDataRoot(new DataRootId("saves"), "D:\\B");

        Assert.Throws<ArgumentException>(() =>
            CreateValidProfile(dataRoots: [rootA, rootB]));
    }

    [Fact]
    public void Constructor_RejectsNonPositiveVersionRetention()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateValidProfile(versionRetention: 0));
    }

    [Fact]
    public void Constructor_RejectsDuplicateExclusionsCaseInsensitively()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateValidProfile(exclusions: ["cache/**", "CACHE/**"]));
    }

    [Fact]
    public void Constructor_AcceptsManagedRecoveryPolicy()
    {
        var recovery = RecoveryPolicy.ManagedCheckpoints(TimeSpan.FromMinutes(10), 2);

        var profile = CreateValidProfile(recoveryPolicy: recovery);

        Assert.Equal(RecoveryMode.ManagedCheckpoints, profile.RecoveryPolicy.Mode);
    }

    private static GameProfile CreateValidProfile(
        bool enabled = true,
        IEnumerable<string>? processNames = null,
        IEnumerable<GameDataRoot>? dataRoots = null,
        IEnumerable<string>? exclusions = null,
        int versionRetention = 5,
        RecoveryPolicy? recoveryPolicy = null)
    {
        return new GameProfile(
            new ProfileId("project-zomboid"),
            "Project Zomboid",
            enabled,
            processNames ?? ["ProjectZomboid64"],
            dataRoots ?? [new GameDataRoot(new DataRootId("saves"), "%USERPROFILE%\\Zomboid")],
            exclusions ?? ["logs/**"],
            versionRetention,
            recoveryPolicy ?? RecoveryPolicy.Disabled,
            externalCloudWarning: true);
    }
}
