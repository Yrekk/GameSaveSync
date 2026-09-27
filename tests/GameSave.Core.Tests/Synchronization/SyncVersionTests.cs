using GameSave.Core.Synchronization;

namespace GameSave.Core.Tests.Synchronization;

public sealed class SyncVersionTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Constructor_RejectsNonPositiveVersion(long value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SyncVersion(value));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(long.MaxValue)]
    public void Constructor_AcceptsPositiveVersion(long value)
    {
        var version = new SyncVersion(value);

        Assert.Equal(value, version.Value);
    }

    [Fact]
    public void Equality_UsesVersionValue()
    {
        Assert.Equal(new SyncVersion(42), new SyncVersion(42));
    }
}
