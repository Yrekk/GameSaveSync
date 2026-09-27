namespace GameSave.Core.Synchronization;

/// <summary>
/// Describes the normal synchronization relationship between local and central state.
/// </summary>
public enum SyncDisposition
{
    Nothing = 0,
    Pull = 1,
    Push = 2,
    Conflict = 3,
    InconsistentState = 4
}
