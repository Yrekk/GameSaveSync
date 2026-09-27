namespace GameSave.Core.Synchronization;

/// <summary>
/// Domain input describing what a machine currently knows about one synchronized profile.
/// </summary>
public readonly record struct LocalSyncState(
    SyncVersion BaseVersion,
    bool IsDirty,
    bool IsGameRunning,
    SaveIntegrityState IntegrityState);
