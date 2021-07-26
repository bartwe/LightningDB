using System;

namespace LightningDB {
    /// <summary>
    ///     Flags to open a database with.
    /// </summary>
    [Flags]
    public enum DatabaseOpenFlags {
        /// <summary>
        ///     No special options.
        /// </summary>
        None = 0,

        /// <summary>
        ///     MDB_REVERSEKEY. Keys are strings to be compared in reverse order, from the end of the strings to the beginning. By
        ///     default, Keys are treated as strings and compared from beginning to end.
        /// </summary>
        ReverseKey = 0x02,

        /// <summary>
        ///     MDB_INTEGERKEY. Keys are binary integers in native byte order.
        ///     Setting this option requires all keys to be the same size, typically sizeof(int) or sizeof(size_t).
        /// </summary>
        IntegerKey = 0x08,

        /// <summary>
        ///     Create the named database if it doesn't exist. This option is not allowed in a read-only transaction or a read-only
        ///     environment.
        /// </summary>
        Create = 0x40000,
    }
}