using GameSave.Application.MetadataDatabase;
using GameSave.Application.Storage;

namespace GameSave.Application.SystemStatus;

/// <summary>
/// Read-only aggregate readiness view used by HTTP, future Admin and later Agent
/// readiness checks. Process liveness is deliberately outside this contract.
/// </summary>
public sealed class GetSystemStatus(
    InspectAndResolveMetadataDatabase inspectAndResolve,
    IMetadataDatabaseSnapshotStore snapshots,
    IGameSaveArtifactStorage storage)
{
    private readonly InspectAndResolveMetadataDatabase _inspectAndResolve =
        inspectAndResolve
        ?? throw new ArgumentNullException(nameof(inspectAndResolve));

    private readonly IMetadataDatabaseSnapshotStore _snapshots =
        snapshots ?? throw new ArgumentNullException(nameof(snapshots));

    private readonly IGameSaveArtifactStorage _storage =
        storage ?? throw new ArgumentNullException(nameof(storage));

    private readonly EvaluateMetadataDatabaseReadiness _readiness = new();

    public async Task<SystemStatus> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var resolved = await _inspectAndResolve.ExecuteAsync(cancellationToken);
        var recoveryAvailability = await GetRecoveryAvailabilityAsync(
            cancellationToken);

        var metadataReadiness = _readiness.Execute(
            resolved.Inspection,
            recoveryAvailability,
            resolved.Classification);

        var storageStatus = await _storage.CheckStatusAsync(cancellationToken);

        var systemMode = metadataReadiness.Mode switch
        {
            MetadataDatabaseOperationalMode.Normal
                when storageStatus == SaveStorageStatus.Ready
                => SystemOperationalMode.Normal,

            MetadataDatabaseOperationalMode.Normal
                => SystemOperationalMode.Maintenance,

            MetadataDatabaseOperationalMode.Maintenance
                => SystemOperationalMode.Maintenance,

            MetadataDatabaseOperationalMode.RestrictedRecovery
                => SystemOperationalMode.RestrictedRecovery,

            MetadataDatabaseOperationalMode.OutOfService
                => SystemOperationalMode.OutOfService,

            _ => throw new ArgumentOutOfRangeException(),
        };

        var synchronizationAvailable =
            metadataReadiness.MetadataAuthorityAvailable
            && storageStatus == SaveStorageStatus.Ready;

        var findings = resolved.Inspection.Findings
            .Concat(resolved.Classification.Findings)
            .Concat(metadataReadiness.Findings)
            .Select(finding => finding.Code)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new SystemStatus(
            systemMode,
            synchronizationAvailable,
            resolved.Classification.EffectiveState,
            metadataReadiness.Mode,
            metadataReadiness.RequiresAdministratorClassification,
            resolved.Classification.EffectiveState
                == MetadataDatabaseState.MigrationRequired,
            recoveryAvailability,
            storageStatus,
            findings);
    }

    private async Task<MetadataRecoveryAvailability>
        GetRecoveryAvailabilityAsync(CancellationToken cancellationToken)
    {
        try
        {
            var snapshots = await _snapshots.ListAsync(cancellationToken);

            return snapshots.Any(snapshot => snapshot.IsValid)
                ? MetadataRecoveryAvailability.Available
                : MetadataRecoveryAvailability.Unavailable;
        }
        catch (IOException)
        {
            return MetadataRecoveryAvailability.Unavailable;
        }
        catch (UnauthorizedAccessException)
        {
            return MetadataRecoveryAvailability.Unavailable;
        }
    }
}
