using GameSave.Core.Machines;

namespace GameSave.Core.Tests.Machines;

public sealed class MachineIdTests
{
    private static readonly Guid SampleValue =
        Guid.Parse("019d2c5e-7f6a-7b21-9b6d-0b6d2f7b2d11");

    [Fact]
    public void Constructor_AcceptsNonEmptyGuid()
    {
        var id = new MachineId(SampleValue);

        Assert.Equal(SampleValue, id.Value);
        Assert.Equal("019d2c5e-7f6a-7b21-9b6d-0b6d2f7b2d11", id.ToString());
    }

    [Fact]
    public void Constructor_RejectsEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => new MachineId(Guid.Empty));
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        Assert.Equal(new MachineId(SampleValue), new MachineId(SampleValue));
    }

    [Fact]
    public void DifferentGuids_AreDifferentMachineIdentities()
    {
        var otherValue = Guid.Parse("019d2c5e-7f6a-7b21-9b6d-0b6d2f7b2d12");

        Assert.NotEqual(new MachineId(SampleValue), new MachineId(otherValue));
    }
}
