using System.Text.Json;
using GameSave.Application.Profiles;
using GameSave.Core.Profiles;
using GameSave.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace GameSave.Persistence.Profiles;

internal sealed class EfGameProfileRepository(GameSaveDbContext dbContext)
    : IGameProfileRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly GameSaveDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<GameProfile?> GetAsync(
        ProfileId profileId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profileId);

        var record = await _dbContext.GameProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.ProfileId == profileId.Value,
                cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<IReadOnlyList<GameProfile>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.GameProfiles
            .AsNoTracking()
            .OrderBy(item => item.ProfileId)
            .ToArrayAsync(cancellationToken);

        return records.Select(Rehydrate).ToArray();
    }

    public async Task SaveAsync(
        GameProfile profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var document = GameProfileDocument.FromDomain(profile);
        var payload = JsonSerializer.Serialize(document, SerializerOptions);

        var record = await _dbContext.GameProfiles
            .SingleOrDefaultAsync(
                item => item.ProfileId == profile.Id.Value,
                cancellationToken);

        if (record is null)
        {
            record = new GameProfileRecord
            {
                ProfileId = profile.Id.Value,
            };
            _dbContext.GameProfiles.Add(record);
        }

        record.DisplayName = profile.DisplayName;
        record.PayloadJson = payload;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static GameProfile Rehydrate(GameProfileRecord record)
    {
        try
        {
            var document = JsonSerializer.Deserialize<GameProfileDocument>(
                record.PayloadJson,
                SerializerOptions)
                ?? throw new InvalidDataException(
                    $"Profile '{record.ProfileId}' has an empty persistence payload.");

            if (!string.Equals(
                    record.ProfileId,
                    document.Id,
                    StringComparison.Ordinal)
                || !string.Equals(
                    record.DisplayName,
                    document.DisplayName,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"Profile '{record.ProfileId}' persistence metadata does not match its payload.");
            }

            return document.ToDomain();
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Profile '{record.ProfileId}' contains invalid persisted JSON.",
                exception);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidDataException(
                $"Profile '{record.ProfileId}' violates GameProfile invariants.",
                exception);
        }
    }
}
