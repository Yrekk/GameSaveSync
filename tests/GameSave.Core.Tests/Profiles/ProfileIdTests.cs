using GameSave.Core.Profiles;

namespace GameSave.Core.Tests.Profiles;

public sealed class ProfileIdTests
{
    [Theory]
    [InlineData("project-zomboid")]
    [InlineData("space-engineers-2")]
    [InlineData("game1")]
    public void Constructor_AcceptsNormalizedSlug(string value)
    {
        var id = new ProfileId(value);

        Assert.Equal(value, id.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Project-Zomboid")]
    [InlineData("project_zomboid")]
    [InlineData("-project-zomboid")]
    [InlineData("project-zomboid-")]
    [InlineData("project--zomboid")]
    public void Constructor_RejectsInvalidSlug(string value)
    {
        Assert.ThrowsAny<ArgumentException>(() => new ProfileId(value));
    }
}
