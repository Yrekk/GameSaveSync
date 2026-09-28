namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Policy-level capabilities allowed by the current metadata readiness state.
/// A capability being allowed does not imply that its operation is already implemented.
/// </summary>
public enum MetadataDatabaseCapability
{
    ViewStatus,
    ViewDiagnostics,
    RetryInspection,
    ResolveClassification,
    UseMetadataAuthority,
    InitializeMetadataDatabase,
    ApplyPendingMigrations,
    DiscoverRecoveryCandidates,
    RestoreMetadataSnapshot,
}
