using GameSave.Application.Inspection;
using GameSave.Application.MetadataDatabase;

namespace GameSave.Application.Tests;

public sealed class MetadataDatabaseReadinessTests
{
    private readonly EvaluateMetadataDatabaseReadiness _evaluator = new();

    [Fact]
    public void Ready_IsNormalAndExposesMetadataAuthority()
    {
        var readiness = _evaluator.Execute(
            CreateInspection(MetadataDatabaseState.Ready));

        Assert.Equal(
            MetadataDatabaseOperationalMode.Normal,
            readiness.Mode);
        Assert.True(readiness.MetadataAuthorityAvailable);
        Assert.False(readiness.RequiresAdministratorClassification);
        Assert.Contains(
            MetadataDatabaseCapability.UseMetadataAuthority,
            readiness.SafeCapabilities);
    }

    [Fact]
    public void AmbiguousInspection_RequiresClassificationAndFailsClosed()
    {
        var inspection = CreateInspection(
            [
                MetadataDatabaseState.Uninitialized,
                MetadataDatabaseState.Invalid,
            ],
            MetadataDatabaseState.Uninitialized);

        var readiness = _evaluator.Execute(
            inspection,
            MetadataRecoveryAvailability.Available);

        Assert.Equal(
            MetadataDatabaseOperationalMode.Maintenance,
            readiness.Mode);
        Assert.False(readiness.MetadataAuthorityAvailable);
        Assert.True(readiness.RequiresAdministratorClassification);
        Assert.Contains(
            MetadataDatabaseCapability.ResolveClassification,
            readiness.SafeCapabilities);
        Assert.DoesNotContain(
            MetadataDatabaseCapability.InitializeMetadataDatabase,
            readiness.SafeCapabilities);
        Assert.DoesNotContain(
            MetadataDatabaseCapability.RestoreMetadataSnapshot,
            readiness.SafeCapabilities);
        Assert.Contains(
            readiness.Findings,
            finding =>
                finding.Code ==
                MetadataDatabaseReadinessFindingCodes.ClassificationRequired);
    }

    [Fact]
    public void Missing_IsMaintenanceAndAllowsExplicitInitialization()
    {
        var readiness = _evaluator.Execute(
            CreateInspection(MetadataDatabaseState.Missing));

        Assert.Equal(
            MetadataDatabaseOperationalMode.Maintenance,
            readiness.Mode);
        Assert.False(readiness.MetadataAuthorityAvailable);
        Assert.Contains(
            MetadataDatabaseCapability.InitializeMetadataDatabase,
            readiness.SafeCapabilities);
        Assert.Contains(
            MetadataDatabaseCapability.DiscoverRecoveryCandidates,
            readiness.SafeCapabilities);
    }

    [Fact]
    public void MissingWithUnavailableRecovery_RemainsMaintenance()
    {
        var readiness = _evaluator.Execute(
            CreateInspection(MetadataDatabaseState.Missing),
            MetadataRecoveryAvailability.Unavailable);

        Assert.Equal(
            MetadataDatabaseOperationalMode.Maintenance,
            readiness.Mode);
        Assert.Contains(
            MetadataDatabaseCapability.InitializeMetadataDatabase,
            readiness.SafeCapabilities);
        Assert.DoesNotContain(
            MetadataDatabaseCapability.RestoreMetadataSnapshot,
            readiness.SafeCapabilities);
    }

    [Fact]
    public void MigrationRequired_IsMaintenanceAndAllowsExplicitMigrationOnly()
    {
        var readiness = _evaluator.Execute(
            CreateInspection(MetadataDatabaseState.MigrationRequired),
            MetadataRecoveryAvailability.Available);

        Assert.Equal(
            MetadataDatabaseOperationalMode.Maintenance,
            readiness.Mode);
        Assert.Contains(
            MetadataDatabaseCapability.ApplyPendingMigrations,
            readiness.SafeCapabilities);
        Assert.DoesNotContain(
            MetadataDatabaseCapability.RestoreMetadataSnapshot,
            readiness.SafeCapabilities);
        Assert.False(readiness.MetadataAuthorityAvailable);
    }

    [Theory]
    [InlineData(MetadataDatabaseState.Invalid)]
    [InlineData(MetadataDatabaseState.TooNew)]
    [InlineData(MetadataDatabaseState.Unavailable)]
    public void UnsafeStateWithUnknownRecovery_IsRestrictedRecovery(
        MetadataDatabaseState state)
    {
        var readiness = _evaluator.Execute(
            CreateInspection(state),
            MetadataRecoveryAvailability.Unknown);

        Assert.Equal(
            MetadataDatabaseOperationalMode.RestrictedRecovery,
            readiness.Mode);
        Assert.Contains(
            MetadataDatabaseCapability.DiscoverRecoveryCandidates,
            readiness.SafeCapabilities);
        Assert.DoesNotContain(
            MetadataDatabaseCapability.RestoreMetadataSnapshot,
            readiness.SafeCapabilities);
        Assert.Contains(
            readiness.Findings,
            finding =>
                finding.Code ==
                MetadataDatabaseReadinessFindingCodes
                    .RecoveryAvailabilityUnknown);
    }

    [Theory]
    [InlineData(MetadataDatabaseState.Invalid)]
    [InlineData(MetadataDatabaseState.TooNew)]
    [InlineData(MetadataDatabaseState.Unavailable)]
    public void UnsafeStateWithAvailableRecovery_AllowsRestorePolicy(
        MetadataDatabaseState state)
    {
        var readiness = _evaluator.Execute(
            CreateInspection(state),
            MetadataRecoveryAvailability.Available);

        Assert.Equal(
            MetadataDatabaseOperationalMode.RestrictedRecovery,
            readiness.Mode);
        Assert.Contains(
            MetadataDatabaseCapability.RestoreMetadataSnapshot,
            readiness.SafeCapabilities);
        Assert.False(readiness.MetadataAuthorityAvailable);
    }

    [Theory]
    [InlineData(MetadataDatabaseState.Invalid)]
    [InlineData(MetadataDatabaseState.TooNew)]
    [InlineData(MetadataDatabaseState.Unavailable)]
    public void UnsafeStateWithUnavailableRecovery_IsOutOfService(
        MetadataDatabaseState state)
    {
        var readiness = _evaluator.Execute(
            CreateInspection(state),
            MetadataRecoveryAvailability.Unavailable);

        Assert.Equal(
            MetadataDatabaseOperationalMode.OutOfService,
            readiness.Mode);
        Assert.False(readiness.MetadataAuthorityAvailable);
        Assert.DoesNotContain(
            MetadataDatabaseCapability.RestoreMetadataSnapshot,
            readiness.SafeCapabilities);
        Assert.DoesNotContain(
            MetadataDatabaseCapability.DiscoverRecoveryCandidates,
            readiness.SafeCapabilities);
        Assert.Contains(
            MetadataDatabaseCapability.ViewStatus,
            readiness.SafeCapabilities);
        Assert.Contains(
            MetadataDatabaseCapability.ViewDiagnostics,
            readiness.SafeCapabilities);
        Assert.Contains(
            MetadataDatabaseCapability.RetryInspection,
            readiness.SafeCapabilities);
        Assert.Contains(
            readiness.Findings,
            finding =>
                finding.Code ==
                MetadataDatabaseReadinessFindingCodes.RecoveryUnavailable);
    }

    [Fact]
    public void UninitializedSingleCandidate_IsMaintenanceForFutureResolvedClassification()
    {
        var readiness = _evaluator.Execute(
            CreateInspection(MetadataDatabaseState.Uninitialized),
            MetadataRecoveryAvailability.Unavailable);

        Assert.Equal(
            MetadataDatabaseOperationalMode.Maintenance,
            readiness.Mode);
        Assert.Contains(
            MetadataDatabaseCapability.InitializeMetadataDatabase,
            readiness.SafeCapabilities);
        Assert.False(readiness.RequiresAdministratorClassification);
    }

    private static MetadataDatabaseInspection CreateInspection(
        MetadataDatabaseState state)
    {
        return CreateInspection([state], state);
    }

    private static MetadataDatabaseInspection CreateInspection(
        IEnumerable<MetadataDatabaseState> states,
        MetadataDatabaseState suggestedState)
    {
        return new MetadataDatabaseInspection(
            new MetadataDatabaseInspectionContext(
                "metadata:test",
                "Test metadata database",
                "revision:test",
                MetadataDatabaseClassificationPolicy.CurrentVersion),
            new MetadataDatabaseInspectionFacts(
                FileExists: true,
                PathOccupiedByNonFile: false,
                Accessible: true,
                IntegrityValid: true,
                HasMigrationHistoryTable: true,
                AppliedMigrationCount: 1,
                UserTableCount: 0,
                CurrentMigration: "current",
                TargetMigration: "target"),
            states,
            suggestedState,
            [new InspectionFinding("database.test")]);
    }
}
