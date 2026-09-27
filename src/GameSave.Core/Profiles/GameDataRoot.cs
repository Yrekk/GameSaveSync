namespace GameSave.Core.Profiles;

/// <summary>
/// Defines one logical group of game data and how its local path is resolved.
/// </summary>
public sealed class GameDataRoot
{
    private readonly IReadOnlyList<MachinePathOverride> _machineOverrides;

    public GameDataRoot(
        DataRootId id,
        string defaultPath,
        IEnumerable<MachinePathOverride>? machineOverrides = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultPath);

        Id = id;
        DefaultPath = defaultPath;

        var overrides = machineOverrides?.ToArray() ?? [];
        var machineIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pathOverride in overrides)
        {
            if (!machineIds.Add(pathOverride.MachineId))
            {
                throw new ArgumentException(
                    $"Machine '{pathOverride.MachineId}' has more than one path override for data root '{id}'.",
                    nameof(machineOverrides));
            }
        }

        _machineOverrides = Array.AsReadOnly(overrides);
    }

    public DataRootId Id { get; }

    public string DefaultPath { get; }

    public IReadOnlyList<MachinePathOverride> MachineOverrides => _machineOverrides;
}
