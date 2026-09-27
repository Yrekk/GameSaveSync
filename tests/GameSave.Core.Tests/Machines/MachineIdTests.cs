using GameSave.Core.Machines;

namespace GameSave.Core.Tests.Machines;

public sealed class MachineIdTests
{
    [Fact]
    public void Constructor_AcceptsOpaqueStableValue()
    {
        var id = new MachineId("01JQ7MACHINE9Y4");

        Assert.Equal("01JQ7MACHINE9Y4", id.Value);
        Assert.Equal("01JQ7MACHINE9Y4", id.ToString());
    }

    [Fact]
    public void Constructor_RejectsWhitespaceOnlyValue()
    {
        Assert.Throws<ArgumentException>(() => new MachineId("   "));
    }

    [Theory]
    [InlineData(" machine-01")]
    [InlineData("machine-01 ")]
    [InlineData("machine 01")]
    public void Constructor_RejectsWhitespaceInsideIdentity(string value)
    {
        Assert.Throws<ArgumentException>(() => new MachineId(value));
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        Assert.Equal(new MachineId("machine-01"), new MachineId("machine-01"));
    }
}
