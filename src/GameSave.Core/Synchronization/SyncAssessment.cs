namespace GameSave.Core.Synchronization;

/// <summary>
/// Complete deterministic result of comparing one local synchronization state with the central state.
/// </summary>
public sealed record SyncAssessment(
    SyncDisposition Disposition,
    SyncFindings Findings)
{
    public bool HasFinding(SyncFindings finding) => (Findings & finding) == finding;
}
