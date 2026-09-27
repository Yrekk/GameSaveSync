namespace GameSave.Core.Profiles;

/// <summary>
/// Configures optional managed recovery checkpoints independently from normal synchronization.
/// </summary>
public sealed record RecoveryPolicy
{
    private RecoveryPolicy(
        RecoveryMode mode,
        TimeSpan checkpointInterval,
        int retentionCount)
    {
        Mode = mode;
        CheckpointInterval = checkpointInterval;
        RetentionCount = retentionCount;
    }

    public static RecoveryPolicy Disabled { get; } =
        new(RecoveryMode.Disabled, TimeSpan.Zero, 0);

    public RecoveryMode Mode { get; }

    public TimeSpan CheckpointInterval { get; }

    public int RetentionCount { get; }

    public static RecoveryPolicy ManagedCheckpoints(
        TimeSpan checkpointInterval,
        int retentionCount)
    {
        if (checkpointInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(checkpointInterval),
                "Managed recovery checkpoint interval must be greater than zero.");
        }

        if (retentionCount < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(retentionCount),
                "Managed recovery must retain at least the current and previous checkpoint.");
        }

        return new RecoveryPolicy(
            RecoveryMode.ManagedCheckpoints,
            checkpointInterval,
            retentionCount);
    }
}
