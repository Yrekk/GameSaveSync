using GameSave.Core.Machines;
using GameSave.Core.Profiles;

namespace GameSave.Core.Tests.Profiles;

public sealed class MachinePathOverrideTests
{
    [Fact]
    public void Constructor_UsesStableMachineIdentity()
    {
        var machineId = new MachineId(
            Guid.Parse("019d2c5e-7f6a-7b21-9b6d-0b6d2f7b2d11"));

        var pathOverride = new MachinePathOverride(machineId, @"D:\GameData");

        Assert.Same(machineId, pathOverride.MachineId);
        Assert.Equal(@"D:\GameData", pathOverride.Path);
    }

    [Fact]
    public void Constructor_RejectsNullMachineIdentity()
    {
        Assert.Throws<ArgumentNullException>(
            () => new MachinePathOverride(null!, @"D:\GameData"));
    }
}
