using GameSave.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Tests;

public sealed class GameSaveDbContextTests
{
    [Fact]
    public void PersistenceEfTypes_AreNotPublicInfrastructureSurface()
    {
        Assert.False(typeof(GameSaveDbContext).IsPublic);
        Assert.False(typeof(GameSaveDbContextDesignFactory).IsPublic);
    }

    [Fact]
    public async Task DbContext_CanOpenSqliteConnection()
    {
        var options = new DbContextOptionsBuilder<GameSaveDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var context = new GameSaveDbContext(options);
        await context.Database.OpenConnectionAsync();

        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", context.Database.ProviderName);
        Assert.True(await context.Database.CanConnectAsync());
    }
}
