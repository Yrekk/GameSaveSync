namespace GameSave.Application.MetadataDatabase;

/// <summary>
/// Capabilities not ruled out by the current metadata readiness state.
/// Authorization, implementation availability and operation-specific preconditions remain downstream concerns.
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
