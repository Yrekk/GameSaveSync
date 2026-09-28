namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Stable control-plane identity for one concrete inspection context.
/// </summary>
public sealed record MetadataDatabaseInspectionContext
{
    public MetadataDatabaseInspectionContext(
        string resourceIdentity,
        string resourceLabel,
        string revision,
        int classificationPolicyVersion)
    {
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

        if (string.IsNullOrWhiteSpace(revision))
        {
            throw new ArgumentException(
                "Inspection revision must not be blank.",
                nameof(revision));
        }

        if (classificationPolicyVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(classificationPolicyVersion),
                "Classification policy version must be positive.");
        }

        ResourceIdentity = resourceIdentity;
        ResourceLabel = resourceLabel;
        Revision = revision;
        ClassificationPolicyVersion = classificationPolicyVersion;
    }

    public string ResourceIdentity { get; }

    public string ResourceLabel { get; }

    public string Revision { get; }

    public int ClassificationPolicyVersion { get; }
}
