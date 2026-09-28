namespace GameSave.Application.Storage;

/// <summary>
/// Opaque identity for one logical stored save artifact set.
/// Publication/version semantics arrive in later transfer tranches.
/// </summary>
public sealed record SaveArtifactId
{
    public SaveArtifactId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Save artifact id must not be the empty GUID.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public override string ToString() => Value.ToString("D");
}
