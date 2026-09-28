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
        bool metadataAuthorityAvailable,
        bool requiresAdministratorClassification,
        IEnumerable<MetadataDatabaseCapability> allowedCapabilities,
        IEnumerable<InspectionFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(allowedCapabilities);
        ArgumentNullException.ThrowIfNull(findings);

        var capabilities = allowedCapabilities.Distinct().ToArray();

        if (metadataAuthorityAvailable
            != (mode == MetadataDatabaseOperationalMode.Normal))
        {
            throw new ArgumentException(
                "Metadata authority is available only in Normal mode.",
                nameof(metadataAuthorityAvailable));
        }

        if (requiresAdministratorClassification
            && mode == MetadataDatabaseOperationalMode.Normal)
        {
            throw new ArgumentException(
                "Normal mode cannot require administrator classification.",
                nameof(requiresAdministratorClassification));
        }

        Mode = mode;
        MetadataAuthorityAvailable = metadataAuthorityAvailable;
        RequiresAdministratorClassification =
            requiresAdministratorClassification;
        SafeCapabilities = Array.AsReadOnly(capabilities);
        Findings = Array.AsReadOnly(findings.ToArray());
    }

    public MetadataDatabaseOperationalMode Mode { get; }

    public bool MetadataAuthorityAvailable { get; }

    public bool RequiresAdministratorClassification { get; }

    public IReadOnlyList<MetadataDatabaseCapability> SafeCapabilities { get; }

    public IReadOnlyList<InspectionFinding> Findings { get; }
}
