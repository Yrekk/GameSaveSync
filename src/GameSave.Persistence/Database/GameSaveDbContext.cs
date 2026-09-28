using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Database;

/// <summary>
/// EF Core metadata database boundary for the central GameSaveSync server.
/// </summary>
internal sealed class GameSaveDbContext(DbContextOptions<GameSaveDbContext> options)
    : DbContext(options)
{
}
