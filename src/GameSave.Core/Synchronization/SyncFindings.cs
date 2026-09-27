namespace GameSave.Core.Synchronization;

/// <summary>
/// Describes all notable safety or consistency conditions detected during one synchronization assessment.
/// Multiple findings may be present at the same time.
/// </summary>
[Flags]
public enum SyncFindings
{
    None = 0,
    GameRunning = 1 << 0,
    LocalIntegrityUnknown = 1 << 1,
    LocalSaveRequiresValidation = 1 << 2,
    LocalSaveInvalid = 1 << 3,
    LocalBaseVersionMissing = 1 << 4,
    CentralVersionMissingForKnownLocalBase = 1 << 5,
    CentralVersionBehindLocalBase = 1 << 6
}
