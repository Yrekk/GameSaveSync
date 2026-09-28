namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Read-only inspection result: observed facts, compatible classifications,
/// a recommendation and the evidence behind that recommendation.
/// </summary>
public sealed class MetadataDatabaseInspection
{
    public MetadataDatabaseInspection(
        MetadataDatabaseInspectionFacts facts,
        IEnumerable<MetadataDatabaseState> candidateStates,
        MetadataDatabaseState suggestedState,
        IEnumerable<string> reasons)
    {
        ArgumentNullException.ThrowIfNull(facts);
        ArgumentNullException.ThrowIfNull(candidateStates);
        ArgumentNullException.ThrowIfNull(reasons);

        var candidates = candidateStates.Distinct().ToArray();

        if (candidates.Length == 0)
        {
            throw new ArgumentException(
                "At least one candidate database state is required.",
                nameof(candidateStates));
        }

        if (!candidates.Contains(suggestedState))
        {
            throw new ArgumentException(
                "The suggested database state must be one of the candidate states.",
                nameof(suggestedState));
        }

        var reasonList = reasons
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .ToArray();

        if (reasonList.Length == 0)
        {
            throw new ArgumentException(
                "At least one inspection reason is required.",
                nameof(reasons));
        }

        Facts = facts;
        CandidateStates = Array.AsReadOnly(candidates);
        SuggestedState = suggestedState;
        Reasons = Array.AsReadOnly(reasonList);
    }

    public MetadataDatabaseInspectionFacts Facts { get; }

    public IReadOnlyList<MetadataDatabaseState> CandidateStates { get; }

    public MetadataDatabaseState SuggestedState { get; }

    public IReadOnlyList<string> Reasons { get; }

    /// <summary>
    /// True when technical facts leave more than one safe semantic
    /// classification available to the administrator.
    /// </summary>
    public bool RequiresAdministratorClassification => CandidateStates.Count > 1;
}
