using GameSave.Persistence.Profiles;
using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Database;

/// <summary>
/// EF Core metadata database boundary for the central GameSaveSync server.
/// </summary>
internal sealed class GameSaveDbContext(DbContextOptions<GameSaveDbContext> options)
    : DbContext(options)
{
    public DbSet<GameProfileRecord> GameProfiles => Set<GameProfileRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var profile = modelBuilder.Entity<GameProfileRecord>();

        profile.ToTable("GameProfiles");
        profile.HasKey(item => item.ProfileId);
        profile.Property(item => item.ProfileId)
            .HasMaxLength(128)
            .IsRequired();
        profile.Property(item => item.DisplayName)
            .HasMaxLength(256)
            .IsRequired();
        profile.Property(item => item.PayloadJson)
            .IsRequired();
    }
}
