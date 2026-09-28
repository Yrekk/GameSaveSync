using GameSave.Application.Inspection;

namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Read-only inspection result: observed facts, compatible classifications,
/// a recommendation and structured evidence.
/// </summary>
public sealed class MetadataDatabaseInspection
{
    public MetadataDatabaseInspection(
        MetadataDatabaseInspectionFacts facts,
        IEnumerable<MetadataDatabaseState> candidateStates,
        MetadataDatabaseState suggestedState,
        IEnumerable<InspectionFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(facts);
        ArgumentNullException.ThrowIfNull(candidateStates);
        ArgumentNullException.ThrowIfNull(findings);

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

        var findingList = findings.ToArray();

        if (findingList.Length == 0)
        {
            throw new ArgumentException(
                "At least one inspection finding is required.",
                nameof(findings));
        }

        Facts = facts;
        CandidateStates = Array.AsReadOnly(candidates);
        SuggestedState = suggestedState;
        Findings = Array.AsReadOnly(findingList);
    }

    public MetadataDatabaseInspectionFacts Facts { get; }

    public IReadOnlyList<MetadataDatabaseState> CandidateStates { get; }

    public MetadataDatabaseState SuggestedState { get; }

    public IReadOnlyList<InspectionFinding> Findings { get; }

    /// <summary>
    /// True when technical facts leave more than one safe semantic
    /// classification available to the administrator.
    /// </summary>
    public bool RequiresAdministratorClassification => CandidateStates.Count > 1;
}
