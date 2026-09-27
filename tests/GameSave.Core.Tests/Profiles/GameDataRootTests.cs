using GameSave.Core.Machines;
using GameSave.Core.Profiles;

namespace GameSave.Core.Tests.Profiles;

public sealed class GameDataRootTests
{
    [Fact]
    public void Constructor_AcceptsPerMachineOverrides()
    {
        var root = new GameDataRoot(
            new DataRootId("saves"),
            "%USERPROFILE%\\Zomboid",
            [
                new MachinePathOverride(
                    new MachineId(Guid.Parse("019d2c5e-7f6a-7b21-9b6d-0b6d2f7b2d11")),
                    "D:\\GameData\\Zomboid"),
                new MachinePathOverride(
                    new MachineId(Guid.Parse("019d2c5e-7f6a-7b21-9b6d-0b6d2f7b2d12")),
                    "E:\\PortableData\\Zomboid")
            ]);

        Assert.Equal(2, root.MachineOverrides.Count);
    }

    [Fact]
    public void Constructor_RejectsNullDataRootId()
    {
        Assert.Throws<ArgumentNullException>(
            () => new GameDataRoot(null!, "%USERPROFILE%\\Zomboid"));
    }

    [Fact]
    public void Constructor_RejectsDuplicateMachineOverridesByIdentity()
    {
        var machineId = Guid.Parse("019d2c5e-7f6a-7b21-9b6d-0b6d2f7b2d11");

        Assert.Throws<ArgumentException>(() =>
            new GameDataRoot(
                new DataRootId("saves"),
                "%USERPROFILE%\\Zomboid",
                [
                    new MachinePathOverride(new MachineId(machineId), "D:\\GameData\\Zomboid"),
                    new MachinePathOverride(new MachineId(machineId), "E:\\Other\\Zomboid")
                ]));
    }

    [Fact]
    public void Constructor_RejectsNullMachineOverride()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GameDataRoot(
                new DataRootId("saves"),
                "%USERPROFILE%\\Zomboid",
                [null!]));
    }
}
