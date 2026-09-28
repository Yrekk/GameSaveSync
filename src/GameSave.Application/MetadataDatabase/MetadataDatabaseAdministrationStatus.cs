namespace GameSave.Application.MetadataDatabase;

public enum MetadataDatabaseAdministrationStatus
{
    Succeeded,
    NotAllowed,
    SnapshotInvalid,
    RestoreFailed,
    FinalStateUnexpected,
}
