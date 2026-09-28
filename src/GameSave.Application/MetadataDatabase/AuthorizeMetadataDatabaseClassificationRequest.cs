namespace GameSave.Application.MetadataDatabase;

public sealed record AuthorizeMetadataDatabaseClassificationRequest
{
    public AuthorizeMetadataDatabaseClassificationRequest(
        string expectedInspectionRevision,
        MetadataDatabaseState selectedState,
        string actorReference,
        string actorLabel,
        string? rationale = null)
    {
        if (string.IsNullOrWhiteSpace(expectedInspectionRevision))
        {
            throw new ArgumentException(
                "Expected inspection revision must not be blank.",
                nameof(expectedInspectionRevision));
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

        ExpectedInspectionRevision = expectedInspectionRevision;
        SelectedState = selectedState;
        ActorReference = actorReference;
        ActorLabel = actorLabel;
        Rationale = string.IsNullOrWhiteSpace(rationale)
            ? null
            : rationale.Trim();
    }

    public string ExpectedInspectionRevision { get; }

    public MetadataDatabaseState SelectedState { get; }

    public string ActorReference { get; }

    public string ActorLabel { get; }

    public string? Rationale { get; }
}
