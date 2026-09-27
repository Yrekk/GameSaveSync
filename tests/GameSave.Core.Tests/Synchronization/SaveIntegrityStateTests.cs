using GameSave.Core.Synchronization;

namespace GameSave.Core.Tests.Synchronization;

public sealed class SaveIntegrityStateTests
{
    [Fact]
    public void DefaultState_IsUnknown()
    {
        Assert.Equal(SaveIntegrityState.Unknown, default(SaveIntegrityState));
    }
}
