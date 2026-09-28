using GameSave.Core.Machines;
using GameSave.Core.Profiles;
using GameSave.Persistence.Database;
using GameSave.Persistence.Profiles;
using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Tests;

public sealed class GameProfileRepositoryTests
{
    [Fact]
    public async Task SaveAndReload_RoundTripsCompleteProfile()
    {
        using var fixture = await RepositoryFixture.CreateAsync();
        var repository = fixture.CreateRepository();
        var machineId = new MachineId(Guid.CreateVersion7());

        var profile = new GameProfile(
            new ProfileId("project-zomboid"),
            "Project Zomboid",
            enabled: true,
            ["ProjectZomboid64.exe", "ProjectZomboid.exe"],
            [
                new GameDataRoot(
                    new DataRootId("saves"),
                    "%USERPROFILE%/Zomboid/Saves",
                    [
                        new MachinePathOverride(
                            machineId,
                            "D:/Games/Zomboid/Saves"),
                    ]),
            ],
            ["Logs/**", "*.tmp"],
            versionRetention: 12,
            RecoveryPolicy.ManagedCheckpoints(
                TimeSpan.FromMinutes(5),
                retentionCount: 2),
            externalCloudWarning: true);

        await repository.SaveAsync(profile);
        var reloaded = await repository.GetAsync(profile.Id);

        Assert.NotNull(reloaded);
        Assert.Equal(profile.Id, reloaded.Id);
        Assert.Equal(profile.DisplayName, reloaded.DisplayName);
        Assert.Equal(profile.ProcessNames, reloaded.ProcessNames);
        Assert.Equal(profile.Exclusions, reloaded.Exclusions);
        Assert.Equal(profile.VersionRetention, reloaded.VersionRetention);
        Assert.Equal(profile.RecoveryPolicy, reloaded.RecoveryPolicy);
        Assert.Equal(profile.ExternalCloudWarning, reloaded.ExternalCloudWarning);

        var root = Assert.Single(reloaded.DataRoots);
        Assert.Equal("saves", root.Id.Value);
        Assert.Equal("%USERPROFILE%/Zomboid/Saves", root.DefaultPath);
        var pathOverride = Assert.Single(root.MachineOverrides);
        Assert.Equal(machineId, pathOverride.MachineId);
        Assert.Equal("D:/Games/Zomboid/Saves", pathOverride.Path);
    }

    [Fact]
    public async Task SaveExistingProfile_UpdatesSameGlobalProfileId()
    {
        using var fixture = await RepositoryFixture.CreateAsync();
        var repository = fixture.CreateRepository();
        var id = new ProfileId("game");

        await repository.SaveAsync(CreateProfile(id, "First"));
        await repository.SaveAsync(CreateProfile(id, "Updated"));

        var profiles = await repository.ListAsync();

        var profile = Assert.Single(profiles);
        Assert.Equal(id, profile.Id);
        Assert.Equal("Updated", profile.DisplayName);
    }

    private static GameProfile CreateProfile(ProfileId id, string displayName)
    {
        return new GameProfile(
            id,
            displayName,
            true,
            ["game.exe"],
            [new GameDataRoot(new DataRootId("main"), "C:/Game/Saves")],
            [],
            5,
            RecoveryPolicy.Disabled,
            false);
    }

    private sealed class RepositoryFixture : IDisposable
    {
        private readonly string _root;
        private readonly DbContextOptions<GameSaveDbContext> _options;

        private RepositoryFixture(
            string root,
            DbContextOptions<GameSaveDbContext> options)
        {
            _root = root;
            _options = options;
        }

        public static async Task<RepositoryFixture> CreateAsync()
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "GameSaveSync.Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            var databasePath = Path.Combine(root, "metadata.db");
            var options = new DbContextOptionsBuilder<GameSaveDbContext>()
                .UseSqlite($"Data Source={databasePath};Pooling=False")
                .Options;

            await using var context = new GameSaveDbContext(options);
            await context.Database.MigrateAsync();

            return new RepositoryFixture(root, options);
        }

        public IGameProfileRepository CreateRepository()
        {
            return new EfGameProfileRepository(
                new GameSaveDbContext(_options));
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
    }
}
