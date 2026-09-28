using GameSave.Application.Inspection;
using GameSave.Application.MetadataDatabase;
using GameSave.Application.Storage;
using GameSave.Application.SystemStatus;

namespace GameSave.Application.Tests;

public sealed class MetadataDatabaseAdministrationTests
{
    [Fact]
    public async Task Initialize_MissingDatabase_ReinspectsAndRequiresReady()
    {
        var provider = new SequenceInspectionProvider(
            CreateInspection(MetadataDatabaseState.Missing, "r1"),
            CreateInspection(MetadataDatabaseState.Ready, "r2"));
        var lifecycle = new FakeLifecycleOperator();
        var useCase = new InitializeMetadataDatabase(
            CreateInspectAndResolve(provider),
            lifecycle);

        var result = await useCase.ExecuteAsync();

        Assert.Equal(
            MetadataDatabaseAdministrationStatus.Succeeded,
            result.Status);
        Assert.Equal(1, lifecycle.InitializeCalls);
        Assert.Equal(MetadataDatabaseState.Ready, result.Inspection.SuggestedState);
    }

    [Fact]
    public async Task ApplyMigrations_CreatesValidatedPreMigrationSnapshotFirst()
    {
        var provider = new SequenceInspectionProvider(
            CreateInspection(MetadataDatabaseState.MigrationRequired, "r1"),
            CreateInspection(MetadataDatabaseState.Ready, "r2"));
        var lifecycle = new FakeLifecycleOperator();
        var snapshots = new FakeSnapshotStore();
        var useCase = new ApplyPendingMetadataDatabaseMigrations(
            CreateInspectAndResolve(provider),
            lifecycle,
            snapshots);

        var result = await useCase.ExecuteAsync();

        Assert.Equal(
            MetadataDatabaseAdministrationStatus.Succeeded,
            result.Status);
        Assert.Equal(
            [MetadataDatabaseSnapshotKind.PreMigration],
            snapshots.CreatedKinds);
        Assert.Equal(1, lifecycle.MigrationCalls);
    }

    [Fact]
    public async Task Restore_ReadyDatabase_IsNotAllowed()
    {
        var provider = new SequenceInspectionProvider(
            CreateInspection(MetadataDatabaseState.Ready, "r1"));
        var snapshots = new FakeSnapshotStore();
        var useCase = new RestoreMetadataDatabaseSnapshot(
            CreateInspectAndResolve(provider),
            snapshots);

        var result = await useCase.ExecuteAsync("snapshot-1");

        Assert.Equal(
            MetadataDatabaseAdministrationStatus.NotAllowed,
            result.Status);
        Assert.Equal(0, snapshots.RestoreCalls);
    }

    [Fact]
    public async Task Restore_UsesExplicitValidCandidateAndReinspects()
    {
        var provider = new SequenceInspectionProvider(
            CreateInspection(MetadataDatabaseState.Invalid, "r1"),
            CreateInspection(MetadataDatabaseState.Ready, "r2"));
        var snapshots = new FakeSnapshotStore
        {
            ListedSnapshots =
            [
                new MetadataDatabaseSnapshotInfo(
                    "snapshot-1",
                    MetadataDatabaseSnapshotKind.Rolling,
                    DateTimeOffset.UtcNow,
                    true),
            ],
        };
        var useCase = new RestoreMetadataDatabaseSnapshot(
            CreateInspectAndResolve(provider),
            snapshots);

        var result = await useCase.ExecuteAsync("snapshot-1");

        Assert.Equal(
            MetadataDatabaseAdministrationStatus.Succeeded,
            result.Status);
        Assert.Equal("snapshot-1", snapshots.LastRestoredId);
        Assert.Equal(1, snapshots.RestoreCalls);
    }

    [Fact]
    public async Task SystemStatus_InvalidMetadataWithoutRecovery_IsOutOfService()
    {
        var provider = new SequenceInspectionProvider(
            CreateInspection(MetadataDatabaseState.Invalid, "r1"));
        var snapshots = new FakeSnapshotStore
        {
            ListedSnapshots = [],
        };
        var storage = new FakeArtifactStorage(SaveStorageStatus.Ready);
        var useCase = new GetSystemStatus(
            CreateInspectAndResolve(provider),
            snapshots,
            storage);

        var status = await useCase.ExecuteAsync();

        Assert.Equal(SystemOperationalMode.OutOfService, status.Mode);
        Assert.False(status.SynchronizationAvailable);
        Assert.Equal(
            MetadataRecoveryAvailability.Unavailable,
            status.RecoveryAvailability);
    }

    private static InspectAndResolveMetadataDatabase CreateInspectAndResolve(
        IMetadataDatabaseInspectionProvider provider)
    {
        var inspect = new InspectMetadataDatabase(provider);
        var store = new EmptyClassificationStore();
        var resolve = new ResolveMetadataDatabaseClassification(store);

        return new InspectAndResolveMetadataDatabase(inspect, resolve);
    }

    private static MetadataDatabaseInspection CreateInspection(
        MetadataDatabaseState state,
        string revision)
    {
        return new MetadataDatabaseInspection(
            new MetadataDatabaseInspectionContext(
                "metadata:test",
                "Test metadata database",
                revision,
                MetadataDatabaseClassificationPolicy.CurrentVersion),
            new MetadataDatabaseInspectionFacts(
                FileExists: state != MetadataDatabaseState.Missing,
                PathOccupiedByNonFile: false,
                Accessible: state is not (
                    MetadataDatabaseState.Missing
                    or MetadataDatabaseState.Unavailable),
                IntegrityValid: state == MetadataDatabaseState.Invalid
                    ? false
                    : state is MetadataDatabaseState.Missing
                        or MetadataDatabaseState.Unavailable
                        ? null
                        : true,
                HasMigrationHistoryTable: state is MetadataDatabaseState.Ready
                    or MetadataDatabaseState.MigrationRequired,
                AppliedMigrationCount: state is MetadataDatabaseState.Ready
                    or MetadataDatabaseState.MigrationRequired
                    ? 1
                    : null,
                UserTableCount: 0,
                CurrentMigration: state == MetadataDatabaseState.Ready
                    ? "target"
                    : state == MetadataDatabaseState.MigrationRequired
                        ? "old"
                        : null,
                TargetMigration: "target"),
            [state],
            state,
            [new InspectionFinding("database.test")]);
    }

    private sealed class SequenceInspectionProvider(
        params MetadataDatabaseInspection[] inspections)
        : IMetadataDatabaseInspectionProvider
    {
        private readonly Queue<MetadataDatabaseInspection> _inspections =
            new(inspections);

        public Task<MetadataDatabaseInspection> InspectAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_inspections.Dequeue());
        }
    }

    private sealed class EmptyClassificationStore
        : IMetadataDatabaseClassificationStore
    {
        public Task<MetadataDatabaseClassificationStoreReadResult> ReadAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                new MetadataDatabaseClassificationStoreReadResult(
                    MetadataDatabaseClassificationStoreStatus.Ready));
        }

        public Task<MetadataDatabaseClassificationStoreStatus> AppendAsync(
            AuthorizedMetadataDatabaseClassification classification,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeLifecycleOperator
        : IMetadataDatabaseLifecycleOperator
    {
        public int InitializeCalls { get; private set; }

        public int MigrationCalls { get; private set; }

        public Task InitializeAsync(
            CancellationToken cancellationToken = default)
        {
            InitializeCalls++;
            return Task.CompletedTask;
        }

        public Task ApplyPendingMigrationsAsync(
            CancellationToken cancellationToken = default)
        {
            MigrationCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSnapshotStore : IMetadataDatabaseSnapshotStore
    {
        public IReadOnlyList<MetadataDatabaseSnapshotInfo> ListedSnapshots
        {
            get;
            init;
        } = [];

        public List<MetadataDatabaseSnapshotKind> CreatedKinds { get; } = [];

        public int RestoreCalls { get; private set; }

        public string? LastRestoredId { get; private set; }

        public Task<MetadataDatabaseSnapshotInfo> CreateAsync(
            MetadataDatabaseSnapshotKind kind,
            CancellationToken cancellationToken = default)
        {
            CreatedKinds.Add(kind);

            return Task.FromResult(
                new MetadataDatabaseSnapshotInfo(
                    $"snapshot-{CreatedKinds.Count}",
                    kind,
                    DateTimeOffset.UtcNow,
                    true));
        }

        public Task<IReadOnlyList<MetadataDatabaseSnapshotInfo>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ListedSnapshots);
        }

        public Task<bool> RestoreAsync(
            string snapshotId,
            CancellationToken cancellationToken = default)
        {
            RestoreCalls++;
            LastRestoredId = snapshotId;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeArtifactStorage(SaveStorageStatus status)
        : IGameSaveArtifactStorage
    {
        public Task WriteAsync(
            SaveArtifactKey key,
            Stream content,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Stream?> OpenReadAsync(
            SaveArtifactKey key,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ExistsAsync(
            SaveArtifactKey key,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SaveStorageStatus> CheckStatusAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(status);
    }
}
