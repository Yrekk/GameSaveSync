namespace GameSave.Core.Synchronization;

/// <summary>
/// Identifies a monotonic synchronization version known by the domain.
/// </summary>
public readonly record struct SyncVersion : IComparable<SyncVersion>
{
    public SyncVersion(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        Value = value;
    }

    public long Value { get; }

    public int CompareTo(SyncVersion other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}
