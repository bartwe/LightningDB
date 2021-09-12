namespace LightningDB {
    /// <summary>
    ///     Cursor operation types
    /// </summary>
    public enum CursorOperation {
        /// <summary>
        ///     Position at first key/data item
        /// </summary>
        First = 0,

        /// <summary>
        ///     Return key/data at current cursor position
        /// </summary>
        GetCurrent = 4,

        /// <summary>
        ///     Position at last key/data item
        /// </summary>
        Last = 6,

        /// <summary>
        ///     Position at next data item
        /// </summary>
        Next = 8,

        /// <summary>
        ///     Position at previous data item
        /// </summary>
        Previous = 12,

        /// <summary>
        ///     Position at specified key
        /// </summary>
        Set = 15,

        /// <summary>
        ///     Position at specified key, return key + data
        /// </summary>
        SetKey = 16,

        /// <summary>
        ///     Position at first key greater than or equal to specified key.
        /// </summary>
        SetRange = 17,
    }
}
