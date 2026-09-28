using GameSave.Application.MetadataDatabase;
using GameSave.Application.Storage;

namespace GameSave.Application.SystemStatus;

public sealed record SystemStatus(
    SystemOperationalMode Mode,
    bool SynchronizationAvailable,
    MetadataDatabaseState? MetadataState,
    MetadataDatabaseOperationalMode MetadataMode,
    bool RequiresAdministratorClassification,
    bool MigrationsPending,
    MetadataRecoveryAvailability RecoveryAvailability,
    SaveStorageStatus StorageStatus,
    IReadOnlyList<string> FindingCodes);
