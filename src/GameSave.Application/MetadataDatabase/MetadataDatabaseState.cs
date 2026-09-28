namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Semantic lifecycle states that may be assigned to the metadata database.
/// Inspection proposes compatible states; it never selects one authoritatively.
/// </summary>
public enum MetadataDatabaseState
{
    Missing,
    Uninitialized,
    Ready,
    MigrationRequired,
    TooNew,
    Unavailable,
    Invalid,
}
