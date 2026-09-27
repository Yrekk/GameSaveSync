using GameSave.Core.Synchronization;

namespace GameSave.Core.Tests.Synchronization;

public sealed class SyncAssessmentEngineTests
{
    [Theory]
    [InlineData(42, false, 42, SyncDisposition.Nothing)]
    [InlineData(42, false, 43, SyncDisposition.Pull)]
    [InlineData(42, true, 42, SyncDisposition.Push)]
    [InlineData(42, true, 43, SyncDisposition.Conflict)]
    public void Assess_ReturnsExpectedDisposition(
        long baseVersion,
        bool isDirty,
        long centralVersion,
        SyncDisposition expectedDisposition)
    {
        var local = new LocalSyncState(
            new SyncVersion(baseVersion),
            isDirty,
            false,
            SaveIntegrityState.Trusted);
        var central = new CentralSyncState(new SyncVersion(centralVersion));

        var assessment = SyncAssessmentEngine.Assess(local, central);

        Assert.Equal(expectedDisposition, assessment.Disposition);
        Assert.Equal(SyncFindings.None, assessment.Findings);
    }

    [Theory]
    [InlineData(false, SyncDisposition.Nothing)]
    [InlineData(true, SyncDisposition.Push)]
    public void Assess_HandlesProfileBeforeFirstCentralPublication(
        bool isDirty,
        SyncDisposition expectedDisposition)
    {
        var local = new LocalSyncState(
            null,
            isDirty,
            false,
            SaveIntegrityState.Trusted);
        var central = new CentralSyncState(null);

        var assessment = SyncAssessmentEngine.Assess(local, central);

        Assert.Equal(expectedDisposition, assessment.Disposition);
        Assert.True(assessment.HasFinding(SyncFindings.LocalBaseVersionMissing));
    }

    [Theory]
    [InlineData(false, SyncDisposition.Pull)]
    [InlineData(true, SyncDisposition.Conflict)]
    public void Assess_HandlesLocalStateWithoutBaseWhenCentralVersionExists(
        bool isDirty,
        SyncDisposition expectedDisposition)
    {
        var local = new LocalSyncState(
            null,
            isDirty,
            false,
            SaveIntegrityState.Trusted);
        var central = new CentralSyncState(new SyncVersion(1));

        var assessment = SyncAssessmentEngine.Assess(local, central);

        Assert.Equal(expectedDisposition, assessment.Disposition);
        Assert.True(assessment.HasFinding(SyncFindings.LocalBaseVersionMissing));
    }

    [Fact]
    public void Assess_ReportsAllSimultaneousFindings()
    {
        var local = new LocalSyncState(
            new SyncVersion(42),
            true,
            true,
            SaveIntegrityState.RequiresValidation);
        var central = new CentralSyncState(new SyncVersion(43));

        var assessment = SyncAssessmentEngine.Assess(local, central);

        Assert.Equal(SyncDisposition.Conflict, assessment.Disposition);
        Assert.True(assessment.HasFinding(SyncFindings.GameRunning));
        Assert.True(assessment.HasFinding(SyncFindings.LocalSaveRequiresValidation));
        Assert.Equal(
            SyncFindings.GameRunning | SyncFindings.LocalSaveRequiresValidation,
            assessment.Findings);
    }

    [Theory]
    [InlineData(SaveIntegrityState.Unknown, SyncFindings.LocalIntegrityUnknown)]
    [InlineData(SaveIntegrityState.RequiresValidation, SyncFindings.LocalSaveRequiresValidation)]
    [InlineData(SaveIntegrityState.Invalid, SyncFindings.LocalSaveInvalid)]
    public void Assess_ReportsIntegrityFinding(
        SaveIntegrityState integrityState,
        SyncFindings expectedFinding)
    {
        var local = new LocalSyncState(
            new SyncVersion(42),
            false,
            false,
            integrityState);
        var central = new CentralSyncState(new SyncVersion(42));

        var assessment = SyncAssessmentEngine.Assess(local, central);

        Assert.True(assessment.HasFinding(expectedFinding));
    }

    [Fact]
    public void Assess_ReportsGameRunningWithoutHidingUnderlyingDisposition()
    {
        var local = new LocalSyncState(
            new SyncVersion(42),
            true,
            true,
            SaveIntegrityState.Trusted);
        var central = new CentralSyncState(new SyncVersion(42));

        var assessment = SyncAssessmentEngine.Assess(local, central);

        Assert.Equal(SyncDisposition.Push, assessment.Disposition);
        Assert.Equal(SyncFindings.GameRunning, assessment.Findings);
    }

    [Fact]
    public void Assess_ReportsInconsistentStateWhenCentralVersionIsMissingForKnownLocalBase()
    {
        var local = new LocalSyncState(
            new SyncVersion(42),
            true,
            true,
            SaveIntegrityState.RequiresValidation);
        var central = new CentralSyncState(null);

        var assessment = SyncAssessmentEngine.Assess(local, central);

        Assert.Equal(SyncDisposition.InconsistentState, assessment.Disposition);
        Assert.True(assessment.HasFinding(SyncFindings.CentralVersionMissingForKnownLocalBase));
        Assert.True(assessment.HasFinding(SyncFindings.GameRunning));
        Assert.True(assessment.HasFinding(SyncFindings.LocalSaveRequiresValidation));
    }

    [Fact]
    public void Assess_ReportsInconsistentStateWhenCentralVersionIsBehindLocalBase()
    {
        var local = new LocalSyncState(
            new SyncVersion(43),
            true,
            true,
            SaveIntegrityState.RequiresValidation);
        var central = new CentralSyncState(new SyncVersion(42));

        var assessment = SyncAssessmentEngine.Assess(local, central);

        Assert.Equal(SyncDisposition.InconsistentState, assessment.Disposition);
        Assert.True(assessment.HasFinding(SyncFindings.CentralVersionBehindLocalBase));
        Assert.True(assessment.HasFinding(SyncFindings.GameRunning));
        Assert.True(assessment.HasFinding(SyncFindings.LocalSaveRequiresValidation));
    }
}
