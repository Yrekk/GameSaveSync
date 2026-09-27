namespace GameSave.Core.Synchronization;

/// <summary>
/// Domain input describing what a machine currently knows about one synchronized profile.
/// A null base version means the local state has never been based on a published central version.
/// </summary>
public readonly record struct LocalSyncState(
    SyncVersion? BaseVersion,
    bool IsDirty,
    bool IsGameRunning,
    SaveIntegrityState IntegrityState);
