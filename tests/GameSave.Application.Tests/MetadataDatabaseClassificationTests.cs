using GameSave.Application.Inspection;
using GameSave.Application.MetadataDatabase;

namespace GameSave.Application.Tests;

public sealed class MetadataDatabaseClassificationTests
{
    [Fact]
    public async Task Resolve_DeterministicInspection_DoesNotReadControlStore()
    {
        var inspection = CreateInspection(
            "revision:ready",
            [MetadataDatabaseState.Ready],
            MetadataDatabaseState.Ready);
        var store = new FakeStore
        {
            ReadResult = new MetadataDatabaseClassificationStoreReadResult(
                MetadataDatabaseClassificationStoreStatus.Invalid),
        };

        var resolution =
            await new ResolveMetadataDatabaseClassification(store)
                .ExecuteAsync(inspection);

        Assert.Equal(
            MetadataDatabaseClassificationAuthority.Deterministic,
            resolution.Authority);
        Assert.Equal(
            MetadataDatabaseState.Ready,
            resolution.EffectiveState);
        Assert.Equal(0, store.ReadCount);
    }

    [Fact]
    public async Task Resolve_AmbiguousInspection_ReusesLatestCompatibleDecision()
    {
        var inspection = CreateAmbiguousInspection("revision:one");
        var first = CreateAuthorized(
            inspection,
            MetadataDatabaseState.Uninitialized,
            "first");
        var second = CreateAuthorized(
            inspection,
            MetadataDatabaseState.Invalid,
            "second",
            "Known foreign fixture.");
        var store = FakeStore.Ready(first, second);

        var resolution =
            await new ResolveMetadataDatabaseClassification(store)
                .ExecuteAsync(inspection);

        Assert.Equal(
            MetadataDatabaseClassificationAuthority.Authorized,
            resolution.Authority);
        Assert.Equal(
            MetadataDatabaseState.Invalid,
            resolution.EffectiveState);
        Assert.Same(second, resolution.AuthorizedClassification);
    }

    [Fact]
    public async Task Resolve_ChangedRevision_DoesNotReuseStaleDecision()
    {
        var oldInspection = CreateAmbiguousInspection("revision:old");
        var currentInspection = CreateAmbiguousInspection("revision:new");
        var store = FakeStore.Ready(
            CreateAuthorized(
                oldInspection,
                MetadataDatabaseState.Uninitialized,
                "old"));

        var resolution =
            await new ResolveMetadataDatabaseClassification(store)
                .ExecuteAsync(currentInspection);

        Assert.Equal(
            MetadataDatabaseClassificationAuthority.Unresolved,
            resolution.Authority);
        Assert.Null(resolution.EffectiveState);
        Assert.Contains(
            resolution.Findings,
            finding =>
                finding.Code ==
                MetadataDatabaseReadinessFindingCodes.ClassificationRequired);
    }

    [Theory]
    [InlineData(
        MetadataDatabaseClassificationStoreStatus.Unavailable,
        MetadataDatabaseClassificationFindingCodes.StoreUnavailable)]
    [InlineData(
        MetadataDatabaseClassificationStoreStatus.Invalid,
        MetadataDatabaseClassificationFindingCodes.StoreInvalid)]
    public async Task Resolve_AmbiguousInspection_ControlStoreFailureFailsClosed(
        MetadataDatabaseClassificationStoreStatus storeStatus,
        string expectedFinding)
    {
        var store = new FakeStore
        {
            ReadResult =
                new MetadataDatabaseClassificationStoreReadResult(storeStatus),
        };

        var resolution =
            await new ResolveMetadataDatabaseClassification(store)
                .ExecuteAsync(CreateAmbiguousInspection("revision:one"));

        Assert.True(resolution.RequiresAdministratorClassification);
        Assert.Null(resolution.EffectiveState);
        Assert.Contains(
            resolution.Findings,
            finding => finding.Code == expectedFinding);
    }

    [Fact]
    public async Task Authorize_StaleExpectedRevision_DoesNotWrite()
    {
        var current = CreateAmbiguousInspection("revision:new");
        var store = FakeStore.Ready();
        var useCase = CreateAuthorizeUseCase(current, store);

        var result = await useCase.ExecuteAsync(
            new AuthorizeMetadataDatabaseClassificationRequest(
                "revision:old",
                MetadataDatabaseState.Uninitialized,
                "admin:damien",
                "Damien Ferrari"));

        Assert.Equal(
            AuthorizeMetadataDatabaseClassificationStatus.StaleInspection,
            result.Status);
        Assert.Empty(store.Writes);
        Assert.Equal("revision:new", result.Inspection.Context.Revision);
    }

    [Fact]
    public async Task Authorize_OverrideWithoutRationale_IsRejected()
    {
        var inspection = CreateAmbiguousInspection("revision:one");
        var store = FakeStore.Ready();
        var useCase = CreateAuthorizeUseCase(inspection, store);

        var result = await useCase.ExecuteAsync(
            new AuthorizeMetadataDatabaseClassificationRequest(
                inspection.Context.Revision,
                MetadataDatabaseState.Invalid,
                "admin:damien",
                "Damien Ferrari"));

        Assert.Equal(
            AuthorizeMetadataDatabaseClassificationStatus.RationaleRequired,
            result.Status);
        Assert.Empty(store.Writes);
    }

    [Fact]
    public async Task Authorize_OverridePersistsHumanAuditContext()
    {
        var inspection = CreateAmbiguousInspection("revision:one");
        var store = FakeStore.Ready();
        var useCase = CreateAuthorizeUseCase(inspection, store);

        var result = await useCase.ExecuteAsync(
            new AuthorizeMetadataDatabaseClassificationRequest(
                inspection.Context.Revision,
                MetadataDatabaseState.Invalid,
                "admin:190992294",
                "Damien Ferrari",
                "Old development fixture; never adopt."));

        Assert.Equal(
            AuthorizeMetadataDatabaseClassificationStatus.Recorded,
            result.Status);

        var persisted = Assert.Single(store.Writes);
        Assert.Equal("admin:190992294", persisted.ActorReference);
        Assert.Equal("Damien Ferrari", persisted.ActorLabel);
        Assert.Equal(
            "GameSaveSync metadata database",
            persisted.ResourceLabel);
        Assert.Equal(
            "Old development fixture; never adopt.",
            persisted.Rationale);
        Assert.Equal(
            new DateTimeOffset(2026, 9, 28, 11, 0, 0, TimeSpan.Zero),
            persisted.DecidedAtUtc);
    }

    [Fact]
    public async Task Authorize_AcceptingSuggestion_DoesNotRequireRationale()
    {
        var inspection = CreateAmbiguousInspection("revision:one");
        var store = FakeStore.Ready();
        var useCase = CreateAuthorizeUseCase(inspection, store);

        var result = await useCase.ExecuteAsync(
            new AuthorizeMetadataDatabaseClassificationRequest(
                inspection.Context.Revision,
                MetadataDatabaseState.Uninitialized,
                "admin:damien",
                "Damien Ferrari"));

        Assert.Equal(
            AuthorizeMetadataDatabaseClassificationStatus.Recorded,
            result.Status);
        Assert.Null(Assert.Single(store.Writes).Rationale);
    }

    [Fact]
    public void Readiness_UsesAuthorizedEffectiveState()
    {
        var inspection = CreateAmbiguousInspection("revision:one");
        var classification = CreateAuthorized(
            inspection,
            MetadataDatabaseState.Invalid,
            "decision",
            "Known foreign fixture.");
        var resolution =
            MetadataDatabaseClassificationResolution.Authorized(
                inspection,
                classification);

        var readiness = new EvaluateMetadataDatabaseReadiness().Execute(
            inspection,
            MetadataRecoveryAvailability.Available,
            resolution);

        Assert.Equal(
            MetadataDatabaseOperationalMode.RestrictedRecovery,
            readiness.Mode);
        Assert.False(readiness.RequiresAdministratorClassification);
        Assert.Contains(
            MetadataDatabaseCapability.RestoreMetadataSnapshot,
            readiness.SafeCapabilities);
        Assert.DoesNotContain(
            MetadataDatabaseCapability.ResolveClassification,
            readiness.SafeCapabilities);
    }

    private static AuthorizeMetadataDatabaseClassification CreateAuthorizeUseCase(
        MetadataDatabaseInspection inspection,
        FakeStore store)
    {
        var provider = new StubProvider(inspection);
        return new AuthorizeMetadataDatabaseClassification(
            new InspectMetadataDatabase(provider),
            store,
            new FixedTimeProvider(
                new DateTimeOffset(
                    2026,
                    9,
                    28,
                    11,
                    0,
                    0,
                    TimeSpan.Zero)));
    }

    private static MetadataDatabaseInspection CreateAmbiguousInspection(
        string revision)
    {
        return CreateInspection(
            revision,
            [
                MetadataDatabaseState.Uninitialized,
                MetadataDatabaseState.Invalid,
            ],
            MetadataDatabaseState.Uninitialized);
    }

    private static MetadataDatabaseInspection CreateInspection(
        string revision,
        IEnumerable<MetadataDatabaseState> candidates,
        MetadataDatabaseState suggestedState)
    {
        return new MetadataDatabaseInspection(
            new MetadataDatabaseInspectionContext(
                "metadata-database:/srv/gamesave/data/gamesave-metadata.db",
                "GameSaveSync metadata database",
                revision,
                MetadataDatabaseClassificationPolicy.CurrentVersion),
            new MetadataDatabaseInspectionFacts(
                FileExists: true,
                PathOccupiedByNonFile: false,
                Accessible: true,
                IntegrityValid: true,
                HasMigrationHistoryTable: false,
                AppliedMigrationCount: 0,
                UserTableCount: 0,
                CurrentMigration: null,
                TargetMigration: "20260928000000_InitialMetadataDatabase"),
            candidates,
            suggestedState,
            [new InspectionFinding("database.test")]);
    }

    private static AuthorizedMetadataDatabaseClassification CreateAuthorized(
        MetadataDatabaseInspection inspection,
        MetadataDatabaseState selectedState,
        string actorLabel,
        string? rationale = null)
    {
        return new AuthorizedMetadataDatabaseClassification(
            Guid.NewGuid(),
            inspection.Context.ResourceIdentity,
            inspection.Context.ResourceLabel,
            inspection.Context.Revision,
            inspection.Context.ClassificationPolicyVersion,
            inspection.CandidateStates,
            inspection.SuggestedState,
            selectedState,
            $"admin:{actorLabel}",
            actorLabel,
            new DateTimeOffset(2026, 9, 28, 10, 0, 0, TimeSpan.Zero),
            rationale);
    }

    private sealed class FakeStore : IMetadataDatabaseClassificationStore
    {
        public MetadataDatabaseClassificationStoreReadResult ReadResult { get; init; } =
            new(MetadataDatabaseClassificationStoreStatus.Ready);

        public MetadataDatabaseClassificationStoreStatus WriteStatus { get; init; } =
            MetadataDatabaseClassificationStoreStatus.Ready;

        public int ReadCount { get; private set; }

        public List<AuthorizedMetadataDatabaseClassification> Writes { get; } = [];

        public Task<MetadataDatabaseClassificationStoreReadResult> ReadAsync(
            CancellationToken cancellationToken = default)
        {
            ReadCount++;
            return Task.FromResult(ReadResult);
        }

        public Task<MetadataDatabaseClassificationStoreStatus> AppendAsync(
            AuthorizedMetadataDatabaseClassification classification,
            CancellationToken cancellationToken = default)
        {
            Writes.Add(classification);
            return Task.FromResult(WriteStatus);
        }

        public static FakeStore Ready(
            params AuthorizedMetadataDatabaseClassification[] history)
        {
            return new FakeStore
            {
                ReadResult = new MetadataDatabaseClassificationStoreReadResult(
                    MetadataDatabaseClassificationStoreStatus.Ready,
                    history),
            };
        }
    }

    private sealed class StubProvider(
        MetadataDatabaseInspection inspection)
        : IMetadataDatabaseInspectionProvider
    {
        public Task<MetadataDatabaseInspection> InspectAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(inspection);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
