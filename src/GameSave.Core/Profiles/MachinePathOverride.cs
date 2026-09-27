namespace GameSave.Core.Profiles;

/// <summary>
/// Overrides one data-root path for a specific machine.
/// </summary>
public readonly record struct MachinePathOverride
{
    public MachinePathOverride(string machineId, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(machineId);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!string.Equals(machineId, machineId.Trim(), StringComparison.Ordinal))
        {
            throw new ArgumentException("Machine id must not contain leading or trailing whitespace.", nameof(machineId));
        }

        MachineId = machineId;
        Path = path;
    }

    public string MachineId { get; }

    public string Path { get; }
}
