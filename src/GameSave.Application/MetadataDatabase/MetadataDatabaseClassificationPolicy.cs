namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Version of the classification contract used to validate durable decisions.
/// Increment only when a policy change can invalidate prior human choices.
/// </summary>
public static class MetadataDatabaseClassificationPolicy
{
    public const int CurrentVersion = 1;
}
