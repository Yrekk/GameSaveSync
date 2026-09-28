namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Semantic lifecycle classifications that may be assigned to the metadata database.
/// Inspection proposes compatible states; it never selects one authoritatively.
/// Invalid can represent detected inconsistency or an authorized rejection of an
/// otherwise technically compatible database for the current GameSaveSync authority.
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
