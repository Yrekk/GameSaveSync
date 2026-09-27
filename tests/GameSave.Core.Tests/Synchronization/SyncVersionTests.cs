using GameSave.Core.Synchronization;

namespace GameSave.Core.Tests.Synchronization;

public sealed class SyncVersionTests
{
    [Fact]
    public void Constructor_RejectsNegativeVersion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SyncVersion(-1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(long.MaxValue)]
    public void Constructor_AcceptsNonNegativeVersion(long value)
    {
        var version = new SyncVersion(value);

        Assert.Equal(value, version.Value);
    }
}
