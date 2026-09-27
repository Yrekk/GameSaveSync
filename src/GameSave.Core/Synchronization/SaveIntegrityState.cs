namespace GameSave.Core.Synchronization;

/// <summary>
/// Describes how much trust the domain may place in the current local save data.
/// </summary>
public enum SaveIntegrityState
{
    /// <summary>
    /// No integrity conclusion is available. This is the safe default and must not be treated as trusted.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// No known integrity concern prevents the save from participating in normal synchronization.
    /// </summary>
    Trusted = 1,

    /// <summary>
    /// An abnormal condition requires explicit validation before the save may be promoted centrally.
    /// </summary>
    RequiresValidation = 2,

    /// <summary>
    /// The save has been confirmed unusable and must not be promoted centrally.
    /// </summary>
    Invalid = 3
}
