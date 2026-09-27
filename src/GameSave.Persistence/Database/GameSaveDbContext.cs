using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Database;

/// <summary>
/// EF Core metadata database boundary for the central GameSaveSync server.
/// </summary>
public sealed class GameSaveDbContext(DbContextOptions<GameSaveDbContext> options)
    : DbContext(options)
{
}
