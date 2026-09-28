namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Canonical Nexus-compatible finding codes used by metadata DB inspection.
/// </summary>
public static class MetadataDatabaseFindingCodes
{
    public const string ResourceMissing = "database.resource.missing";
    public const string ResourcePathNotFile = "database.resource.path_not_file";
    public const string ResourceUnavailable = "database.resource.unavailable";
    public const string IntegrityFailed = "database.integrity.failed";
    public const string SchemaUninitialized = "database.schema.uninitialized";
    public const string ApplicationHistoryAbsent =
        "database.schema.application_history_absent";
    public const string UserObjectsAbsent = "database.schema.user_objects_absent";
    public const string NonApplicationObjectsPresent =
        "database.schema.non_application_objects_present";
    public const string SchemaCurrent = "database.schema.current";
    public const string SchemaOutdated = "database.schema.outdated";
    public const string SchemaNewerThanRuntime =
        "database.schema.newer_than_runtime";
    public const string SchemaHistoryInconsistent =
        "database.schema.history_inconsistent";
    public const string SchemaReadFailed = "database.schema.read_failed";
}
