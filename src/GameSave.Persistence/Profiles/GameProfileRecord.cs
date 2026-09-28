namespace GameSave.Persistence.Profiles;

/// <summary>
/// Minimal EF row for one complete GameProfile aggregate.
/// PayloadJson is versioned application persistence, while DisplayName is
/// intentionally duplicated for human diagnostics and simple listings.
/// </summary>
internal sealed class GameProfileRecord
{
    public string ProfileId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;
}
