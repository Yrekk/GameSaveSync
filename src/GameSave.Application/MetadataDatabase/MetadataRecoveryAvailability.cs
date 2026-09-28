namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Current knowledge about whether a safe metadata recovery path exists.
/// H2.1D models this evidence without implementing snapshot discovery.
/// </summary>
public enum MetadataRecoveryAvailability
{
    Unknown,
    Available,
    Unavailable,
}
