using GameSave.Core.Machines;
using GameSave.Core.Profiles;

namespace GameSave.Core.Tests.Profiles;

public sealed class MachinePathOverrideTests
{
    [Fact]
    public void Constructor_UsesStableMachineIdentity()
    {
        var machineId = new MachineId("machine-01");

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
