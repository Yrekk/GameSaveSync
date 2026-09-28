namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Stable Nexus-compatible findings produced by readiness evaluation.
/// </summary>
public static class MetadataDatabaseReadinessFindingCodes
{
    public const string ClassificationRequired =
        "database.classification.required";

    public const string RecoveryAvailabilityUnknown =
        "database.recovery.availability_unknown";

    public const string RecoveryUnavailable =
        "database.recovery.unavailable";
}
