using GameSave.Application.MetadataDatabase;
using GameSave.Application.Storage;
using GameSave.Application.SystemStatus;
using GameSave.Server.SystemStatus;
using Xunit;

namespace GameSave.Server.Tests;

public sealed class SystemStatusContractMapperTests
{
    [Fact]
    public void ToResponse_MapsApplicationStatusWithoutLeakingDomainTypes()
    {
        var status = new SystemStatus(
            SystemOperationalMode.RestrictedRecovery,
            false,
            MetadataDatabaseState.Invalid,
            MetadataDatabaseOperationalMode.RestrictedRecovery,
            false,
            false,
            MetadataRecoveryAvailability.Available,
            SaveStorageStatus.Ready,
            ["database.integrity.failed"]);

        var response = SystemStatusContractMapper.ToResponse(status);

        Assert.Equal("RestrictedRecovery", response.Mode);
        Assert.False(response.SynchronizationAvailable);
        Assert.Equal("Invalid", response.Metadata.State);
        Assert.Equal("Available", response.Metadata.RecoveryAvailability);
        Assert.Equal("Ready", response.Storage.Status);
        Assert.Equal(
            ["database.integrity.failed"],
            response.FindingCodes);
    }
}
