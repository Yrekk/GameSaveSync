namespace GameSave.Core.Profiles;

/// <summary>
/// Stable normalized identifier for a game profile.
/// </summary>
public readonly record struct ProfileId
{
    public ProfileId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!IsNormalizedSlug(value))
        {
            throw new ArgumentException(
                "Profile id must be a lowercase slug containing only letters, digits and single hyphen separators.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;

    private static bool IsNormalizedSlug(string value)
    {
        if (value[0] == '-' || value[^1] == '-')
        {
            return false;
        }

        var previousWasHyphen = false;

        foreach (var character in value)
        {
            var isLowerLetter = character is >= 'a' and <= 'z';
            var isDigit = character is >= '0' and <= '9';
            var isHyphen = character == '-';

            if (!isLowerLetter && !isDigit && !isHyphen)
            {
                return false;
            }

            if (isHyphen && previousWasHyphen)
            {
                return false;
            }

            previousWasHyphen = isHyphen;
        }

        return true;
    }
}
