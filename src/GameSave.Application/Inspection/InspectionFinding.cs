using System.Collections;
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

        var copiedDetails = new Dictionary<string, object?>();

        if (details is not null)
        {
            foreach (var (key, value) in details)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentException(
                        "Inspection finding detail keys must not be empty.",
                        nameof(details));
                }

                copiedDetails[key] = CopyTransportSafeValue(key, value, details);
            }
        }

        Code = code;
        Details = new ReadOnlyDictionary<string, object?>(copiedDetails);
    }

    public string Code { get; }

    public IReadOnlyDictionary<string, object?> Details { get; }

    private static object? CopyTransportSafeValue(
        string key,
        object? value,
        IReadOnlyDictionary<string, object?> details)
    {
        if (IsTransportSafeScalar(value))
        {
            return value;
        }

        if (value is IList values)
        {
            var copiedValues = new object?[values.Count];

            for (var index = 0; index < values.Count; index++)
            {
                var item = values[index];

                if (!IsTransportSafeScalar(item))
                {
                    throw UnsupportedDetailValue(key, details);
                }

                copiedValues[index] = item;
            }

            return Array.AsReadOnly(copiedValues);
        }

        throw UnsupportedDetailValue(key, details);
    }

    private static bool IsTransportSafeScalar(object? value)
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

    private static ArgumentException UnsupportedDetailValue(
        string key,
        IReadOnlyDictionary<string, object?> details)
    {
        return new ArgumentException(
            $"Inspection finding detail '{key}' has an unsupported value type.",
            nameof(details));
    }
}
