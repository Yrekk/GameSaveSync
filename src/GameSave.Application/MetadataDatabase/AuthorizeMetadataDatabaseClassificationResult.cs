using GameSave.Application.Inspection;

namespace GameSave.Application.MetadataDatabase;

public sealed class AuthorizeMetadataDatabaseClassificationResult
{
    public AuthorizeMetadataDatabaseClassificationResult(
        AuthorizeMetadataDatabaseClassificationStatus status,
        MetadataDatabaseInspection inspection,
        AuthorizedMetadataDatabaseClassification? classification = null,
        IEnumerable<InspectionFinding>? findings = null)
    {
        ArgumentNullException.ThrowIfNull(inspection);

        if (status == AuthorizeMetadataDatabaseClassificationStatus.Recorded
            && classification is null)
        {
            throw new ArgumentException(
                "Recorded result requires the persisted classification.",
                nameof(classification));
        }

        if (status != AuthorizeMetadataDatabaseClassificationStatus.Recorded
            && classification is not null)
        {
            throw new ArgumentException(
                "Only a recorded result may expose a persisted classification.",
                nameof(classification));
        }

        Status = status;
        Inspection = inspection;
        Classification = classification;
        Findings = Array.AsReadOnly((findings ?? []).ToArray());
    }

    public AuthorizeMetadataDatabaseClassificationStatus Status { get; }

    public MetadataDatabaseInspection Inspection { get; }

    public AuthorizedMetadataDatabaseClassification? Classification { get; }

    public IReadOnlyList<InspectionFinding> Findings { get; }
}
