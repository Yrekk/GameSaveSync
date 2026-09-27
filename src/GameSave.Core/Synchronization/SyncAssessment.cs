namespace GameSave.Core.Synchronization;

/// <summary>
/// Complete deterministic result of comparing one local synchronization state with the central state.
/// </summary>
public readonly record struct SyncAssessment(
    SyncDisposition Disposition,
    SyncFindings Findings)
{
    public bool HasFinding(SyncFindings finding) => (Findings & finding) == finding;
}
