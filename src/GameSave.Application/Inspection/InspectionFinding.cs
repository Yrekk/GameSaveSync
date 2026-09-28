using System.Collections.ObjectModel;

namespace GameSave.Application.Inspection;

/// <summary>
/// Stable machine-readable evidence produced by an inspection.
/// Human-readable wording belongs to presentation adapters.
/// </summary>
public sealed class InspectionFinding
{
    public InspectionFinding(
        string code,
        IReadOnlyDictionary<string, object?>? details = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "Inspection finding code must not be empty.",
                nameof(code));
        }

        var copiedDetails = details is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(details);

        foreach (var (key, value) in copiedDetails)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "Inspection finding detail keys must not be empty.",
                    nameof(details));
            }

            if (!IsTransportSafeValue(value))
            {
                throw new ArgumentException(
                    $"Inspection finding detail '{key}' has an unsupported value type.",
                    nameof(details));
            }
        }

        Code = code;
        Details = new ReadOnlyDictionary<string, object?>(copiedDetails);
    }

    public string Code { get; }

    public IReadOnlyDictionary<string, object?> Details { get; }

    private static bool IsTransportSafeValue(object? value)
    {
        return value is null
            or string
            or bool
            or byte
            or sbyte
            or short
            or ushort
            or int
            or uint
            or long
            or ulong
            or float
            or double
            or decimal;
    }
}
