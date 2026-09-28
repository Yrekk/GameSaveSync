using GameSave.Application.Inspection;

namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Provider-neutral operational view derived from metadata inspection and
/// current recovery evidence.
/// </summary>
public sealed class MetadataDatabaseReadiness
{
    internal MetadataDatabaseReadiness(
        MetadataDatabaseOperationalMode mode,
        bool synchronizationAuthorityAvailable,
        bool requiresAdministratorClassification,
        IEnumerable<MetadataDatabaseCapability> allowedCapabilities,
        IEnumerable<InspectionFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(allowedCapabilities);
        ArgumentNullException.ThrowIfNull(findings);

        var capabilities = allowedCapabilities.Distinct().ToArray();

        if (synchronizationAuthorityAvailable
            != (mode == MetadataDatabaseOperationalMode.Normal))
        {
            throw new ArgumentException(
                "Synchronization authority is available only in Normal mode.",
                nameof(synchronizationAuthorityAvailable));
        }

        if (requiresAdministratorClassification
            && mode == MetadataDatabaseOperationalMode.Normal)
        {
            throw new ArgumentException(
                "Normal mode cannot require administrator classification.",
                nameof(requiresAdministratorClassification));
        }

        Mode = mode;
        SynchronizationAuthorityAvailable = synchronizationAuthorityAvailable;
        RequiresAdministratorClassification =
            requiresAdministratorClassification;
        AllowedCapabilities = Array.AsReadOnly(capabilities);
        Findings = Array.AsReadOnly(findings.ToArray());
    }

    public MetadataDatabaseOperationalMode Mode { get; }

    public bool SynchronizationAuthorityAvailable { get; }

    public bool RequiresAdministratorClassification { get; }

    public IReadOnlyList<MetadataDatabaseCapability> AllowedCapabilities { get; }

    public IReadOnlyList<InspectionFinding> Findings { get; }
}
