using GameSave.Core.Profiles;

namespace GameSave.Core.Tests.Profiles;

public sealed class RecoveryPolicyTests
{
    [Fact]
    public void Disabled_HasNoCheckpointConfiguration()
    {
        var policy = RecoveryPolicy.Disabled;

        Assert.Equal(RecoveryMode.Disabled, policy.Mode);
        Assert.Equal(TimeSpan.Zero, policy.CheckpointInterval);
        Assert.Equal(0, policy.RetentionCount);
    }

    [Fact]
    public void ManagedCheckpoints_AcceptsValidConfiguration()
    {
        var policy = RecoveryPolicy.ManagedCheckpoints(TimeSpan.FromMinutes(10), 2);

        Assert.Equal(RecoveryMode.ManagedCheckpoints, policy.Mode);
        Assert.Equal(TimeSpan.FromMinutes(10), policy.CheckpointInterval);
        Assert.Equal(2, policy.RetentionCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ManagedCheckpoints_RejectsNonPositiveInterval(int minutes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RecoveryPolicy.ManagedCheckpoints(TimeSpan.FromMinutes(minutes), 2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void ManagedCheckpoints_RejectsRetentionBelowTwo(int retentionCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RecoveryPolicy.ManagedCheckpoints(TimeSpan.FromMinutes(10), retentionCount));
    }
}
