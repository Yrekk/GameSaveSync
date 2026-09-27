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
                new MachinePathOverride("pc-fixe", "D:\\GameData\\Zomboid"),
                new MachinePathOverride("laptop", "E:\\PortableData\\Zomboid")
            ]);

        Assert.Equal(2, root.MachineOverrides.Count);
    }

    [Fact]
    public void Constructor_RejectsDuplicateMachineOverridesCaseInsensitively()
    {
        Assert.Throws<ArgumentException>(() =>
            new GameDataRoot(
                new DataRootId("saves"),
                "%USERPROFILE%\\Zomboid",
                [
                    new MachinePathOverride("PC-FIXE", "D:\\GameData\\Zomboid"),
                    new MachinePathOverride("pc-fixe", "E:\\Other\\Zomboid")
                ]));
    }
}
