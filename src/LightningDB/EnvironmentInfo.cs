namespace LightningDB;

/// <summary>
///     Information about the environment.
/// </summary>
public struct EnvironmentInfo {
    /// <summary>
    ///     ID of the last used page
    /// </summary>
    public long LastPageNumber;

    /// <summary>
    ///     ID of the last committed transaction
    /// </summary>
    public long LastTransactionId;

    /// <summary>
    ///     Size of the data memory map
    /// </summary>
    public long MapSize;
}
