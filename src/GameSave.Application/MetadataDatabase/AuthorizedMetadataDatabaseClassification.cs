namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Durable audit record for one authorized ambiguous metadata classification.
/// </summary>
public sealed class AuthorizedMetadataDatabaseClassification
{
    public AuthorizedMetadataDatabaseClassification(
        Guid decisionId,
        string resourceIdentity,
        string resourceLabel,
        string inspectionRevision,
        int classificationPolicyVersion,
        IEnumerable<MetadataDatabaseState> candidateStates,
        MetadataDatabaseState suggestedState,
        MetadataDatabaseState selectedState,
        string actorReference,
        string actorLabel,
        DateTimeOffset decidedAtUtc,
        string? rationale)
    {
        if (decisionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Decision id must not be empty.",
                nameof(decisionId));
        }

        if (string.IsNullOrWhiteSpace(resourceIdentity))
        {
            throw new ArgumentException(
                "Resource identity must not be blank.",
                nameof(resourceIdentity));
        }

        if (string.IsNullOrWhiteSpace(resourceLabel))
        {
            throw new ArgumentException(
                "Resource label must not be blank.",
                nameof(resourceLabel));
        }

        if (string.IsNullOrWhiteSpace(inspectionRevision))
        {
            throw new ArgumentException(
                "Inspection revision must not be blank.",
                nameof(inspectionRevision));
        }

        if (classificationPolicyVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(classificationPolicyVersion),
                "Classification policy version must be positive.");
        }

        ArgumentNullException.ThrowIfNull(candidateStates);

        var candidates = candidateStates.Distinct().ToArray();

        if (candidates.Length < 2)
        {
            throw new ArgumentException(
                "Authorized classification is only persisted for ambiguous inspections.",
                nameof(candidateStates));
        }

        if (!candidates.Contains(suggestedState))
        {
            throw new ArgumentException(
                "Suggested state must be one of the candidate states.",
                nameof(suggestedState));
        }

        if (!candidates.Contains(selectedState))
        {
            throw new ArgumentException(
                "Selected state must be one of the candidate states.",
                nameof(selectedState));
        }

        if (string.IsNullOrWhiteSpace(actorReference))
        {
            throw new ArgumentException(
                "Actor reference must not be blank.",
                nameof(actorReference));
        }

        if (string.IsNullOrWhiteSpace(actorLabel))
        {
            throw new ArgumentException(
                "Actor label must not be blank.",
                nameof(actorLabel));
        }

        if (decidedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Decision timestamp must be UTC.",
                nameof(decidedAtUtc));
        }

        if (selectedState != suggestedState
            && string.IsNullOrWhiteSpace(rationale))
        {
            throw new ArgumentException(
                "A rationale is required when overriding the suggested state.",
                nameof(rationale));
        }

        DecisionId = decisionId;
        ResourceIdentity = resourceIdentity;
        ResourceLabel = resourceLabel;
        InspectionRevision = inspectionRevision;
        ClassificationPolicyVersion = classificationPolicyVersion;
        CandidateStates = Array.AsReadOnly(candidates);
        SuggestedState = suggestedState;
        SelectedState = selectedState;
        ActorReference = actorReference;
        ActorLabel = actorLabel;
        DecidedAtUtc = decidedAtUtc;
        Rationale = string.IsNullOrWhiteSpace(rationale)
            ? null
            : rationale.Trim();
    }

    public Guid DecisionId { get; }

    public string ResourceIdentity { get; }

    public string ResourceLabel { get; }

    public string InspectionRevision { get; }

    public int ClassificationPolicyVersion { get; }

    public IReadOnlyList<MetadataDatabaseState> CandidateStates { get; }

    public MetadataDatabaseState SuggestedState { get; }

    public MetadataDatabaseState SelectedState { get; }

    public string ActorReference { get; }

    public string ActorLabel { get; }

    public DateTimeOffset DecidedAtUtc { get; }

    public string? Rationale { get; }
}
