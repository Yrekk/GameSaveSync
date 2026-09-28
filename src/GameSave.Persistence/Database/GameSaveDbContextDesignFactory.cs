using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GameSave.Persistence.Database;

/// <summary>
/// Design-time factory used only for authoring versioned EF migrations.
/// </summary>
public sealed class GameSaveDbContextDesignFactory
    : IDesignTimeDbContextFactory<GameSaveDbContext>
{
    public GameSaveDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<GameSaveDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        return new GameSaveDbContext(options);
    }
}
