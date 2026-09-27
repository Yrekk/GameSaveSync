namespace GameSave.Core.Machines;

/// <summary>
/// Stable opaque identity of one logical GameSaveSync machine.
/// </summary>
public sealed record MachineId
{
    public MachineId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException(
                "Machine id must not contain whitespace.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
