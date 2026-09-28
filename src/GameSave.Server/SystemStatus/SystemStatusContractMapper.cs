using GameSave.Application.SystemStatus;
using GameSave.Contracts.SystemStatus;

namespace GameSave.Server.SystemStatus;

internal static class SystemStatusContractMapper
{
    public static SystemStatusResponse ToResponse(
        GameSave.Application.SystemStatus.SystemStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        return new SystemStatusResponse(
            status.Mode.ToString(),
            status.SynchronizationAvailable,
            new MetadataStatusResponse(
                status.MetadataState?.ToString(),
                status.MetadataMode.ToString(),
                status.RequiresAdministratorClassification,
                status.MigrationsPending,
                status.RecoveryAvailability.ToString()),
            new StorageStatusResponse(status.StorageStatus.ToString()),
            status.FindingCodes);
    }
}
