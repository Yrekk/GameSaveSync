using GameSave.Core.Profiles;

namespace GameSave.Application.Storage;

/// <summary>
/// Provider-neutral address of one file inside a logical save artifact set.
/// </summary>
public sealed record SaveArtifactKey
{
    public SaveArtifactKey(
        ProfileId profileId,
        SaveArtifactId artifactId,
        DataRootId dataRootId,
        string relativePath)
    {
        ArgumentNullException.ThrowIfNull(profileId);
        ArgumentNullException.ThrowIfNull(artifactId);
        ArgumentNullException.ThrowIfNull(dataRootId);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var normalized = relativePath.Replace('\\', '/').Trim();

        if (Path.IsPathRooted(normalized)
            || normalized.StartsWith("/", StringComparison.Ordinal)
            || normalized.Split(
                    '/',
                    StringSplitOptions.RemoveEmptyEntries)
                .Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException(
                "Artifact path must be a safe relative path without traversal segments.",
                nameof(relativePath));
        }

        ProfileId = profileId;
        ArtifactId = artifactId;
        DataRootId = dataRootId;
        RelativePath = normalized;
    }

    public ProfileId ProfileId { get; }

    public SaveArtifactId ArtifactId { get; }

    public DataRootId DataRootId { get; }

    public string RelativePath { get; }
}
