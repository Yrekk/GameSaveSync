namespace GameSave.Core.Synchronization;

/// <summary>
/// Domain input describing the current version published by the central authority.
/// </summary>
public readonly record struct CentralSyncState(SyncVersion CurrentVersion);
