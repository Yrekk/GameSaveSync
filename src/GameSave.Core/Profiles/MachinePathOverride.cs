using GameSave.Core.Machines;

namespace GameSave.Core.Profiles;

/// <summary>
/// Overrides one data-root path for a specific logical GameSaveSync machine.
/// </summary>
public sealed record MachinePathOverride
{
    public MachinePathOverride(MachineId machineId, string path)
    {
        ArgumentNullException.ThrowIfNull(machineId);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        MachineId = machineId;
        Path = path;
    }

    public MachineId MachineId { get; }

    public string Path { get; }
}
