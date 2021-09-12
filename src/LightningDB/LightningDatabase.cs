using System;
using static LightningDB.Native.Lmdb;

namespace LightningDB {
    /// <summary>
    ///     Lightning database.
    /// </summary>
    public sealed class LightningDatabase : IDisposable {
        uint _handle;
        readonly DatabaseConfiguration _configuration;
        readonly bool _closeOnDispose;
        readonly LightningTransaction _transaction;
        readonly IDisposable _pinnedConfig;

        /// <summary>
        ///     Creates a LightningDatabase instance.
        /// </summary>
        /// <param name="name">Database name.</param>
        /// <param name="transaction">Active transaction.</param>
        /// <param name="configuration">Options for the database, like encoding, option flags, and comparison logic.</param>
        /// <param name="closeOnDispose">Close database handle on dispose</param>
        internal LightningDatabase(string name, LightningTransaction transaction, DatabaseConfiguration configuration, bool closeOnDispose) {
            Name = name;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _closeOnDispose = closeOnDispose;
            Environment = transaction.Environment;
            _transaction = transaction;
            mdb_dbi_open(transaction.Handle(), name, _configuration.Flags, out _handle).ThrowOnError();
            _pinnedConfig = _configuration.ConfigureDatabase(transaction, this);
            IsOpened = true;
        }

        /// <summary>
        ///     Whether the database handle has been release from Dispose, or from unsuccessful OpenDatabase call.
        /// </summary>
        public bool IsReleased {
            get { return _handle == default; }
        }

        /// <summary>
        ///     Is database opened.
        /// </summary>
        public bool IsOpened { get; private set; }

        public Stats DatabaseStats {
            get {
                mdb_stat(_transaction.Handle(), Handle(), out var nativeStat).ThrowOnError();
                return new() {
                    BranchPages = nativeStat.ms_branch_pages.ToInt64(),
                    BTreeDepth = nativeStat.ms_depth,
                    Entries = nativeStat.ms_entries.ToInt64(),
                    LeafPages = nativeStat.ms_leaf_pages.ToInt64(),
                    OverflowPages = nativeStat.ms_overflow_pages.ToInt64(),
                    PageSize = nativeStat.ms_psize,
                };
            }
        }

        /// <summary>
        ///     Database name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Environment in which the database was opened.
        /// </summary>
        public LightningEnvironment Environment { get; }

        /// <summary>
        ///     Deallocates resources opened by the database.
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }

        public uint Handle() {
            return _handle;
        }

        /// <summary>
        ///     Drops the database.
        /// </summary>
        public MDBResultCode Drop(LightningTransaction transaction) {
            var result = mdb_drop(transaction.Handle(), _handle, true);
            IsOpened = false;
            _handle = default;
            return result;
        }

        /// <summary>
        ///     Truncates all data from the database.
        /// </summary>
        public MDBResultCode Truncate(LightningTransaction transaction) {
            return mdb_drop(transaction.Handle(), _handle, false);
        }

        /// <summary>
        ///     Deallocates resources opened by the database.
        /// </summary>
        /// <param name="disposing">true if called from Dispose.</param>
        void Dispose(bool disposing) {
            if (_handle == default) {
                return;
            }

            if (!disposing) {
                throw new InvalidOperationException("The LightningDatabase was not disposed and cannot be reliably dealt with from the finalizer");
            }

            IsOpened = false;
            _pinnedConfig.Dispose();

            if (_closeOnDispose) {
                mdb_dbi_close(Environment.Handle(), _handle);
            }

            _handle = default;
        }

#if DEBUG
        ~LightningDatabase() {
            if (_handle == default) {
                return;
            }
            throw new("Leaked LightningDatabase instance.");
        }
#endif
    }
}
