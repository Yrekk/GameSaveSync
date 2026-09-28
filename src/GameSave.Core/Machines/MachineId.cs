namespace GameSave.Core.Machines;

/// <summary>
/// Stable opaque identity of one logical GameSaveSync machine.
/// </summary>
public sealed record MachineId
{
    public MachineId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Machine id must not be the empty GUID.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public override string ToString() => Value.ToString("D");
}
