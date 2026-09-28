using GameSave.Core.Machines;
using GameSave.Core.Profiles;

namespace GameSave.Persistence.Profiles;

/// <summary>
/// Versioned persistence document for the complete GameProfile aggregate.
/// Keep this DTO infrastructure-owned so Core remains persistence-agnostic.
/// </summary>
internal sealed record GameProfileDocument(
    int SchemaVersion,
    string Id,
    string DisplayName,
    bool Enabled,
    IReadOnlyList<string> ProcessNames,
    IReadOnlyList<GameDataRootDocument> DataRoots,
    IReadOnlyList<string> Exclusions,
    int VersionRetention,
    RecoveryPolicyDocument RecoveryPolicy,
    bool ExternalCloudWarning)
{
    public const int CurrentSchemaVersion = 1;

    public static GameProfileDocument FromDomain(GameProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new GameProfileDocument(
            CurrentSchemaVersion,
            profile.Id.Value,
            profile.DisplayName,
            profile.Enabled,
            profile.ProcessNames.ToArray(),
            profile.DataRoots.Select(
                root => new GameDataRootDocument(
                    root.Id.Value,
                    root.DefaultPath,
                    root.MachineOverrides.Select(
                        pathOverride => new MachinePathOverrideDocument(
                            pathOverride.MachineId.Value,
                            pathOverride.Path))
                        .ToArray()))
                .ToArray(),
            profile.Exclusions.ToArray(),
            profile.VersionRetention,
            RecoveryPolicyDocument.FromDomain(profile.RecoveryPolicy),
            profile.ExternalCloudWarning);
    }

    public GameProfile ToDomain()
    {
        if (SchemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidDataException(
                $"Unsupported GameProfile persistence schema version '{SchemaVersion}'.");
        }

        return new GameProfile(
            new ProfileId(Id),
            DisplayName,
            Enabled,
            ProcessNames,
            DataRoots.Select(
                root => new GameDataRoot(
                    new DataRootId(root.Id),
                    root.DefaultPath,
                    root.MachineOverrides.Select(
                        pathOverride => new MachinePathOverride(
                            new MachineId(pathOverride.MachineId),
                            pathOverride.Path)))),
            Exclusions,
            VersionRetention,
            RecoveryPolicy.ToDomain(),
            ExternalCloudWarning);
    }
}

internal sealed record GameDataRootDocument(
    string Id,
    string DefaultPath,
    IReadOnlyList<MachinePathOverrideDocument> MachineOverrides);

internal sealed record MachinePathOverrideDocument(
    Guid MachineId,
    string Path);

internal sealed record RecoveryPolicyDocument(
    RecoveryMode Mode,
    long CheckpointIntervalTicks,
    int RetentionCount)
{
    public static RecoveryPolicyDocument FromDomain(RecoveryPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return new RecoveryPolicyDocument(
            policy.Mode,
            policy.CheckpointInterval.Ticks,
            policy.RetentionCount);
    }

    public RecoveryPolicy ToDomain()
    {
        return Mode switch
        {
            RecoveryMode.Disabled => RecoveryPolicy.Disabled,
            RecoveryMode.ManagedCheckpoints =>
                RecoveryPolicy.ManagedCheckpoints(
                    TimeSpan.FromTicks(CheckpointIntervalTicks),
                    RetentionCount),
            _ => throw new InvalidDataException(
                $"Unsupported recovery mode '{Mode}'."),
        };
    }
}
