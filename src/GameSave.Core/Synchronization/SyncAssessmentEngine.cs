namespace GameSave.Core.Synchronization;

/// <summary>
/// Evaluates synchronization state without performing any filesystem, network or UI operation.
/// </summary>
public static class SyncAssessmentEngine
{
    public static SyncAssessment Assess(
        LocalSyncState local,
        CentralSyncState central)
    {
        var findings = AssessFindings(local, central);
        var disposition = AssessDisposition(local, central);

        return new SyncAssessment(disposition, findings);
    }

    private static SyncFindings AssessFindings(
        LocalSyncState local,
        CentralSyncState central)
    {
        var findings = SyncFindings.None;

        if (local.IsGameRunning)
        {
            findings |= SyncFindings.GameRunning;
        }

        findings |= local.IntegrityState switch
        {
            SaveIntegrityState.Unknown => SyncFindings.LocalIntegrityUnknown,
            SaveIntegrityState.Trusted => SyncFindings.None,
            SaveIntegrityState.RequiresValidation => SyncFindings.LocalSaveRequiresValidation,
            SaveIntegrityState.Invalid => SyncFindings.LocalSaveInvalid,
            _ => SyncFindings.LocalIntegrityUnknown
        };

        if (central.CurrentVersion.CompareTo(local.BaseVersion) < 0)
        {
            findings |= SyncFindings.CentralVersionBehindLocalBase;
        }

        return findings;
    }

    private static SyncDisposition AssessDisposition(
        LocalSyncState local,
        CentralSyncState central)
    {
        var versionComparison = central.CurrentVersion.CompareTo(local.BaseVersion);

        if (versionComparison < 0)
        {
            return SyncDisposition.InconsistentState;
        }

        if (versionComparison == 0)
        {
            return local.IsDirty
                ? SyncDisposition.Push
                : SyncDisposition.Nothing;
        }

        return local.IsDirty
            ? SyncDisposition.Conflict
            : SyncDisposition.Pull;
    }
}
