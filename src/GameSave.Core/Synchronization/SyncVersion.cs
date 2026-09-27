namespace GameSave.Core.Synchronization;

/// <summary>
/// Identifies one real published synchronization version.
/// Absence of a version is represented separately by a null version reference.
/// </summary>
public sealed record SyncVersion : IComparable<SyncVersion>
{
    public SyncVersion(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Synchronization versions must be strictly positive.");
        }

        Value = value;
    }

    public long Value { get; }

    public int CompareTo(SyncVersion? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Value.CompareTo(other.Value);
    }

    public override string ToString() => Value.ToString();
}
