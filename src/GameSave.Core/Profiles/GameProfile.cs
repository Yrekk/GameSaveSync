namespace GameSave.Core.Profiles;

/// <summary>
/// Complete structurally valid definition of one synchronizable game profile.
/// </summary>
public sealed class GameProfile
{
    private readonly IReadOnlyList<string> _processNames;
    private readonly IReadOnlyList<GameDataRoot> _dataRoots;
    private readonly IReadOnlyList<string> _exclusions;

    public GameProfile(
        ProfileId id,
        string displayName,
        bool enabled,
        IEnumerable<string> processNames,
        IEnumerable<GameDataRoot> dataRoots,
        IEnumerable<string>? exclusions,
        int versionRetention,
        RecoveryPolicy recoveryPolicy,
        bool externalCloudWarning)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(processNames);
        ArgumentNullException.ThrowIfNull(dataRoots);
        ArgumentNullException.ThrowIfNull(recoveryPolicy);

        if (versionRetention <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(versionRetention),
                "Version retention must be greater than zero.");
        }

        var processes = ValidateDistinctNonEmptyStrings(
            processNames,
            nameof(processNames),
            requireAtLeastOne: true);

        var roots = dataRoots.ToArray();
        if (roots.Length == 0)
        {
            throw new ArgumentException(
                "A game profile must define at least one data root.",
                nameof(dataRoots));
        }

        var rootIds = new HashSet<DataRootId>();
        foreach (var root in roots)
        {
            ArgumentNullException.ThrowIfNull(root);

            if (!rootIds.Add(root.Id))
            {
                throw new ArgumentException(
                    $"Data root id '{root.Id}' is duplicated.",
                    nameof(dataRoots));
            }
        }

        var exclusionValues = ValidateDistinctNonEmptyStrings(
            exclusions ?? [],
            nameof(exclusions),
            requireAtLeastOne: false);

        Id = id;
        DisplayName = displayName.Trim();
        Enabled = enabled;
        VersionRetention = versionRetention;
        RecoveryPolicy = recoveryPolicy;
        ExternalCloudWarning = externalCloudWarning;

        _processNames = processes;
        _dataRoots = Array.AsReadOnly(roots);
        _exclusions = exclusionValues;
    }

    public ProfileId Id { get; }

    public string DisplayName { get; }

    public bool Enabled { get; }

    public IReadOnlyList<string> ProcessNames => _processNames;

    public IReadOnlyList<GameDataRoot> DataRoots => _dataRoots;

    public IReadOnlyList<string> Exclusions => _exclusions;

    public int VersionRetention { get; }

    public RecoveryPolicy RecoveryPolicy { get; }

    public bool ExternalCloudWarning { get; }

    private static IReadOnlyList<string> ValidateDistinctNonEmptyStrings(
        IEnumerable<string> values,
        string parameterName,
        bool requireAtLeastOne)
    {
        var normalizedValues = new List<string>();
        var uniqueValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var value in values)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

            var normalized = value.Trim();

            if (!uniqueValues.Add(normalized))
            {
                throw new ArgumentException(
                    $"Value '{normalized}' is duplicated.",
                    parameterName);
            }

            normalizedValues.Add(normalized);
        }

        if (requireAtLeastOne && normalizedValues.Count == 0)
        {
            throw new ArgumentException(
                "At least one value is required.",
                parameterName);
        }

        return normalizedValues.AsReadOnly();
    }
}
