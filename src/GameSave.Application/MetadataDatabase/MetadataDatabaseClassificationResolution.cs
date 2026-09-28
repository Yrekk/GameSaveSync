using GameSave.Application.Inspection;

namespace GameSave.Application.MetadataDatabase;

public sealed class MetadataDatabaseClassificationResolution
{
    private MetadataDatabaseClassificationResolution(
        string inspectionRevision,
        MetadataDatabaseClassificationAuthority authority,
        MetadataDatabaseState? effectiveState,
        AuthorizedMetadataDatabaseClassification? authorizedClassification,
        IEnumerable<InspectionFinding>? findings)
    {
        if (string.IsNullOrWhiteSpace(inspectionRevision))
        {
            throw new ArgumentException(
                "Inspection revision must not be blank.",
                nameof(inspectionRevision));
        }

        var findingList = (findings ?? []).ToArray();

        switch (authority)
        {
            case MetadataDatabaseClassificationAuthority.Deterministic:
                if (effectiveState is null || authorizedClassification is not null)
                {
                    throw new ArgumentException(
                        "Deterministic resolution requires an effective state and no authorized record.",
                        nameof(authority));
                }
                break;

            case MetadataDatabaseClassificationAuthority.Authorized:
                if (effectiveState is null || authorizedClassification is null)
                {
                    throw new ArgumentException(
                        "Authorized resolution requires an effective state and an authorized record.",
                        nameof(authority));
                }
                break;

            case MetadataDatabaseClassificationAuthority.Unresolved:
                if (effectiveState is not null || authorizedClassification is not null)
                {
                    throw new ArgumentException(
                        "Unresolved classification must not expose an effective state.",
                        nameof(authority));
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(authority));
        }

        InspectionRevision = inspectionRevision;
        Authority = authority;
        EffectiveState = effectiveState;
        AuthorizedClassification = authorizedClassification;
        Findings = Array.AsReadOnly(findingList);
    }

    public string InspectionRevision { get; }

    public MetadataDatabaseClassificationAuthority Authority { get; }

    public MetadataDatabaseState? EffectiveState { get; }

    public AuthorizedMetadataDatabaseClassification? AuthorizedClassification { get; }

    public IReadOnlyList<InspectionFinding> Findings { get; }

    public bool RequiresAdministratorClassification =>
        Authority == MetadataDatabaseClassificationAuthority.Unresolved;

    public static MetadataDatabaseClassificationResolution Deterministic(
        MetadataDatabaseInspection inspection)
    {
        ArgumentNullException.ThrowIfNull(inspection);

        if (inspection.CandidateStates.Count != 1)
        {
            throw new ArgumentException(
                "Deterministic resolution requires exactly one candidate state.",
                nameof(inspection));
        }

        return new MetadataDatabaseClassificationResolution(
            inspection.Context.Revision,
            MetadataDatabaseClassificationAuthority.Deterministic,
            inspection.CandidateStates[0],
            null,
            null);
    }

    public static MetadataDatabaseClassificationResolution Authorized(
        MetadataDatabaseInspection inspection,
        AuthorizedMetadataDatabaseClassification classification)
    {
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentNullException.ThrowIfNull(classification);

        var sameCandidates = classification.CandidateStates
            .OrderBy(state => state)
            .SequenceEqual(
                inspection.CandidateStates.OrderBy(state => state));

        if (!inspection.RequiresAdministratorClassification
            || !string.Equals(
                classification.ResourceIdentity,
                inspection.Context.ResourceIdentity,
                StringComparison.Ordinal)
            || !string.Equals(
                classification.InspectionRevision,
                inspection.Context.Revision,
                StringComparison.Ordinal)
            || classification.ClassificationPolicyVersion
                != inspection.Context.ClassificationPolicyVersion
            || classification.SuggestedState != inspection.SuggestedState
            || !inspection.CandidateStates.Contains(classification.SelectedState)
            || !sameCandidates)
        {
            throw new ArgumentException(
                "Authorized classification is incompatible with the inspection context.",
                nameof(classification));
        }

        return new MetadataDatabaseClassificationResolution(
            inspection.Context.Revision,
            MetadataDatabaseClassificationAuthority.Authorized,
            classification.SelectedState,
            classification,
            null);
    }

    public static MetadataDatabaseClassificationResolution Unresolved(
        MetadataDatabaseInspection inspection,
        IEnumerable<InspectionFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentNullException.ThrowIfNull(findings);

        return new MetadataDatabaseClassificationResolution(
            inspection.Context.Revision,
            MetadataDatabaseClassificationAuthority.Unresolved,
            null,
            null,
            findings);
    }
}
