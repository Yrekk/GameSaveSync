namespace GameSave.Contracts.SystemStatus;

/// <summary>
/// Stable transport shape for GET /api/system/status.
/// Strings deliberately carry application vocabulary without exposing
/// Application/Core types across the network boundary.
/// </summary>
public sealed record SystemStatusResponse(
    string Mode,
    bool SynchronizationAvailable,
    MetadataStatusResponse Metadata,
    StorageStatusResponse Storage,
    IReadOnlyList<string> FindingCodes);

public sealed record MetadataStatusResponse(
    string? State,
    string Mode,
    bool RequiresAdministratorClassification,
    bool MigrationsPending,
    string RecoveryAvailability);

public sealed record StorageStatusResponse(string Status);
