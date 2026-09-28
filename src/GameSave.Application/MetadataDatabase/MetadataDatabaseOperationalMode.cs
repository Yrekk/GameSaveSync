namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Safe operating mode for metadata authority.
/// Process liveness is a separate concern.
/// </summary>
public enum MetadataDatabaseOperationalMode
{
    Normal,
    Maintenance,
    RestrictedRecovery,
    OutOfService,
}
