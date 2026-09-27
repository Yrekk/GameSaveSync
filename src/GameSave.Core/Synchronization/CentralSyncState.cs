namespace GameSave.Core.Synchronization;

/// <summary>
/// Domain input describing the current version published by the central authority.
/// A null current version means no central version has been published yet.
/// </summary>
public readonly record struct CentralSyncState(SyncVersion? CurrentVersion);
