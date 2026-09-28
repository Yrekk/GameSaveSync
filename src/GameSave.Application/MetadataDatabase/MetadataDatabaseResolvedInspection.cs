namespace GameSave.Application.MetadataDatabase;

public sealed record MetadataDatabaseResolvedInspection(
    MetadataDatabaseInspection Inspection,
    MetadataDatabaseClassificationResolution Classification);
