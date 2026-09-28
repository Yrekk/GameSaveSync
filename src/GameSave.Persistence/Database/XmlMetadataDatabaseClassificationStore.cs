using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using GameSave.Application.MetadataDatabase;

namespace GameSave.Persistence.Database;

internal sealed class XmlMetadataDatabaseClassificationStore
    : IMetadataDatabaseClassificationStore
{
    private const int DocumentVersion = 1;
    private readonly MetadataDatabaseControlStoreSettings _settings;
    private readonly SemaphoreSlim _writeGate = new(1, 1);

    public XmlMetadataDatabaseClassificationStore(
        MetadataDatabaseControlStoreSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<MetadataDatabaseClassificationStoreReadResult> ReadAsync(
        CancellationToken cancellationToken = default)
    {
        var read = await ReadDocumentAsync(cancellationToken);
        return read.Status == MetadataDatabaseClassificationStoreStatus.Ready
            ? new MetadataDatabaseClassificationStoreReadResult(
                read.Status,
                read.History)
            : new MetadataDatabaseClassificationStoreReadResult(read.Status);
    }

    public async Task<MetadataDatabaseClassificationStoreStatus> AppendAsync(
        AuthorizedMetadataDatabaseClassification classification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(classification);

        await _writeGate.WaitAsync(cancellationToken);

        try
        {
            var read = await ReadDocumentAsync(cancellationToken);

            if (read.Status != MetadataDatabaseClassificationStoreStatus.Ready)
            {
                return read.Status;
            }

            var document = read.Document ?? CreateEmptyDocument();
            var classifications = document
                .Root!
                .Element("AuthorizedClassifications")!;

            classifications.Add(Serialize(classification));

            var path = _settings.ControlFilePath;
            var directory = Path.GetDirectoryName(path);

            if (string.IsNullOrWhiteSpace(directory))
            {
                return MetadataDatabaseClassificationStoreStatus.Unavailable;
            }

            Directory.CreateDirectory(directory);

            var tempPath =
                $"{path}.{Guid.NewGuid():N}.tmp";

            try
            {
                await WriteDocumentAsync(
                    document,
                    tempPath,
                    cancellationToken);

                File.Move(tempPath, path, overwrite: true);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }

            return MetadataDatabaseClassificationStoreStatus.Ready;
        }
        catch (IOException)
        {
            return MetadataDatabaseClassificationStoreStatus.Unavailable;
        }
        catch (UnauthorizedAccessException)
        {
            return MetadataDatabaseClassificationStoreStatus.Unavailable;
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private async Task<ReadDocumentResult> ReadDocumentAsync(
        CancellationToken cancellationToken)
    {
        var path = _settings.ControlFilePath;

        if (!File.Exists(path))
        {
            return new ReadDocumentResult(
                MetadataDatabaseClassificationStoreStatus.Ready,
                CreateEmptyDocument(),
                []);
        }

        try
        {
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            var document = await XDocument.LoadAsync(
                stream,
                LoadOptions.None,
                cancellationToken);

            var history = Parse(document);

            return new ReadDocumentResult(
                MetadataDatabaseClassificationStoreStatus.Ready,
                document,
                history);
        }
        catch (XmlException)
        {
            return Invalid();
        }
        catch (FormatException)
        {
            return Invalid();
        }
        catch (InvalidDataException)
        {
            return Invalid();
        }
        catch (IOException)
        {
            return Unavailable();
        }
        catch (UnauthorizedAccessException)
        {
            return Unavailable();
        }
    }

    private static IReadOnlyList<AuthorizedMetadataDatabaseClassification> Parse(
        XDocument document)
    {
        var root = document.Root;

        if (root is null
            || root.Name != "GameSaveControl"
            || (int?)root.Attribute("version") != DocumentVersion)
        {
            throw new InvalidDataException(
                "Unsupported or invalid GameSave control-plane XML.");
        }

        var container = root.Element("AuthorizedClassifications")
            ?? throw new InvalidDataException(
                "AuthorizedClassifications element is required.");

        var history = new List<AuthorizedMetadataDatabaseClassification>();

        foreach (var element in container.Elements("Classification"))
        {
            var candidates = element
                .Element("CandidateStates")?
                .Elements("State")
                .Select(state => ParseState(state.Value))
                .ToArray()
                ?? throw new InvalidDataException(
                    "CandidateStates element is required.");

            var actor = element.Element("Actor")
                ?? throw new InvalidDataException("Actor element is required.");
            var resource = element.Element("Resource")
                ?? throw new InvalidDataException("Resource element is required.");

            history.Add(
                new AuthorizedMetadataDatabaseClassification(
                    Guid.Parse(RequiredValue(element, "DecisionId")),
                    RequiredValue(resource, "Identity"),
                    RequiredValue(resource, "Label"),
                    RequiredValue(element, "InspectionRevision"),
                    int.Parse(
                        RequiredValue(
                            element,
                            "ClassificationPolicyVersion"),
                        CultureInfo.InvariantCulture),
                    candidates,
                    ParseState(RequiredValue(element, "SuggestedState")),
                    ParseState(RequiredValue(element, "SelectedState")),
                    RequiredValue(actor, "Reference"),
                    RequiredValue(actor, "Label"),
                    DateTimeOffset.Parse(
                        RequiredValue(element, "DecidedAtUtc"),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal
                            | DateTimeStyles.AdjustToUniversal),
                    OptionalValue(element, "Rationale")));
        }

        return history;
    }

    private static XElement Serialize(
        AuthorizedMetadataDatabaseClassification classification)
    {
        return new XElement(
            "Classification",
            new XElement("DecisionId", classification.DecisionId),
            new XElement(
                "Resource",
                new XElement("Identity", classification.ResourceIdentity),
                new XElement("Label", classification.ResourceLabel)),
            new XElement(
                "InspectionRevision",
                classification.InspectionRevision),
            new XElement(
                "ClassificationPolicyVersion",
                classification.ClassificationPolicyVersion),
            new XElement(
                "CandidateStates",
                classification.CandidateStates.Select(
                    state => new XElement("State", state))),
            new XElement("SuggestedState", classification.SuggestedState),
            new XElement("SelectedState", classification.SelectedState),
            new XElement(
                "Actor",
                new XElement("Reference", classification.ActorReference),
                new XElement("Label", classification.ActorLabel)),
            new XElement(
                "DecidedAtUtc",
                classification.DecidedAtUtc.ToString(
                    "O",
                    CultureInfo.InvariantCulture)),
            classification.Rationale is null
                ? null
                : new XElement("Rationale", classification.Rationale));
    }

    private static XDocument CreateEmptyDocument()
    {
        return new XDocument(
            new XElement(
                "GameSaveControl",
                new XAttribute("version", DocumentVersion),
                new XElement("AuthorizedClassifications")));
    }

    private static async Task WriteDocumentAsync(
        XDocument document,
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            FileOptions.Asynchronous | FileOptions.WriteThrough);

        var settings = new XmlWriterSettings
        {
            Async = true,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            NewLineChars = Environment.NewLine,
            NewLineHandling = NewLineHandling.Replace,
        };

        await using var writer = XmlWriter.Create(stream, settings);
        await document.SaveAsync(writer, cancellationToken);
        await writer.FlushAsync();
        await stream.FlushAsync(cancellationToken);
        stream.Flush(flushToDisk: true);
    }

    private static MetadataDatabaseState ParseState(string value)
    {
        return Enum.TryParse<MetadataDatabaseState>(
            value,
            ignoreCase: false,
            out var state)
            ? state
            : throw new InvalidDataException(
                $"Unknown metadata database state '{value}'.");
    }

    private static string RequiredValue(XElement parent, string name)
    {
        var value = parent.Element(name)?.Value;

        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidDataException(
                $"Required XML value '{name}' is missing.")
            : value;
    }

    private static string? OptionalValue(XElement parent, string name)
    {
        var value = parent.Element(name)?.Value;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static ReadDocumentResult Invalid()
    {
        return new ReadDocumentResult(
            MetadataDatabaseClassificationStoreStatus.Invalid,
            null,
            []);
    }

    private static ReadDocumentResult Unavailable()
    {
        return new ReadDocumentResult(
            MetadataDatabaseClassificationStoreStatus.Unavailable,
            null,
            []);
    }

    private sealed record ReadDocumentResult(
        MetadataDatabaseClassificationStoreStatus Status,
        XDocument? Document,
        IReadOnlyList<AuthorizedMetadataDatabaseClassification> History);
}
