using GameSave.Core.Profiles;

namespace GameSave.Application.Profiles;

/// <summary>
/// Persistence port for complete, already-valid game profiles.
/// </summary>
public interface IGameProfileRepository
{
    Task<GameProfile?> GetAsync(
        ProfileId profileId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameProfile>> ListAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        GameProfile profile,
        CancellationToken cancellationToken = default);
}
