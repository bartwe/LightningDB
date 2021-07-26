using System;
using static LightningDB.Native.Lmdb;

namespace LightningDB {
    /// <summary>
    ///     Cursor to iterate over a database
    /// </summary>
    public struct LightningCursor : IDisposable {
        private IntPtr _handle;

        /// <summary>
        ///     Creates new instance of LightningCursor
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="txn">Transaction</param>
        internal LightningCursor(LightningDatabase db, LightningTransaction txn) {
            if (db == null) {
                throw new ArgumentNullException(nameof(db));
            }

            mdb_cursor_open(txn.Handle(), db.Handle(), out _handle).ThrowOnError();

            Transaction = txn;
        }

        /// <summary>
        ///     Gets the the native handle of the cursor
        /// </summary>
        public IntPtr Handle() {
            return _handle;
        }

        /// <summary>
        ///     Cursor's transaction.
        /// </summary>
        public LightningTransaction Transaction { get; }

        /// <summary>
        ///     Position at specified key, if key is not found index will be positioned to closest match.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns <see cref="MDBResultCode" /></returns>
        public MDBResultCode Set(ReadOnlySpan<byte> key) {
            return Get(CursorOperation.Set, key).resultCode;
        }

        /// <summary>
        ///     Moves to the key and populates Current with the values stored.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns <see cref="MDBResultCode" />, and <see cref="MDBValue" /> key/value</returns>
        public (MDBResultCode resultCode, MDBValue key, MDBValue value) SetKey(ReadOnlySpan<byte> key) {
            return Get(CursorOperation.SetKey, key);
        }

        /// <summary>
        ///     Position at first key greater than or equal to specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns <see cref="MDBResultCode" /></returns>
        public MDBResultCode SetRange(ReadOnlySpan<byte> key) {
            return Get(CursorOperation.SetRange, key).resultCode;
        }

        /// <summary>
        ///     Position at first key/data item
        /// </summary>
        /// <returns>Returns <see cref="MDBResultCode" /></returns>
        public MDBResultCode First() {
            return Get(CursorOperation.First).resultCode;
        }

        /// <summary>
        ///     Position at last key/data item
        /// </summary>
        /// <returns>Returns <see cref="MDBResultCode" /></returns>
        public MDBResultCode Last() {
            return Get(CursorOperation.Last).resultCode;
        }

        /// <summary>
        ///     Return key/data at current cursor position
        /// </summary>
        /// <returns>Key/data at current cursor position</returns>
        public (MDBResultCode resultCode, MDBValue key, MDBValue value) GetCurrent() {
            return Get(CursorOperation.GetCurrent);
        }

        /// <summary>
        ///     Position at next data item
        /// </summary>
        /// <returns>Returns <see cref="MDBResultCode" /></returns>
        public MDBResultCode Next() {
            return Get(CursorOperation.Next).resultCode;
        }

        /// <summary>
        ///     Position at previous data item.
        /// </summary>
        /// <returns>Returns <see cref="MDBResultCode" /></returns>
        public MDBResultCode Previous() {
            return Get(CursorOperation.Previous).resultCode;
        }

        private (MDBResultCode resultCode, MDBValue key, MDBValue value) Get(CursorOperation operation) {
            var mdbKey = new MDBValue();
            var mdbValue = new MDBValue();
            return (mdb_cursor_get(_handle, ref mdbKey, ref mdbValue, operation), mdbKey, mdbValue);
        }

        private unsafe (MDBResultCode resultCode, MDBValue key, MDBValue value) Get(CursorOperation operation, ReadOnlySpan<byte> key) {
            fixed (byte* keyPtr = key) {
                var mdbKey = new MDBValue(key.Length, keyPtr);
                var mdbValue = new MDBValue();
                return (mdb_cursor_get(_handle, ref mdbKey, ref mdbValue, operation), mdbKey, mdbValue);
            }
        }

        private unsafe (MDBResultCode resultCode, MDBValue key, MDBValue value) Get(CursorOperation operation, ReadOnlySpan<byte> key, ReadOnlySpan<byte> value) {
            fixed (byte* keyPtr = key)
            fixed (byte* valPtr = value) {
                var mdbKey = new MDBValue(key.Length, keyPtr);
                var mdbValue = new MDBValue(value.Length, valPtr);
                return (mdb_cursor_get(_handle, ref mdbKey, ref mdbValue, operation), mdbKey, mdbValue);
            }
        }


        /// <summary>
        ///     Store by cursor.
        ///     This function stores key/data pairs into the database. The cursor is positioned at the new item, or on failure
        ///     usually near it.
        ///     Note: Earlier documentation incorrectly said errors would leave the state of the cursor unchanged.
        ///     If the function fails for any reason, the state of the cursor will be unchanged.
        ///     If the function succeeds and an item is inserted into the database, the cursor is always positioned to refer to the
        ///     newly inserted item.
        /// </summary>
        /// <param name="key">The key operated on.</param>
        /// <param name="value">The data operated on.</param>
        /// <param name="options">
        ///     Options for this operation. This parameter must be set to 0 or one of the values described here.
        ///     CursorPutOptions.Current - overwrite the data of the key/data pair to which the cursor refers with the specified
        ///     data item. The key parameter is ignored.
        ///     CursorPutOptions.NoDuplicateData - enter the new key/data pair only if it does not already appear in the database.
        ///     This flag may only be specified if the database was opened with MDB_DUPSORT. The function will return MDB_KEYEXIST
        ///     if the key/data pair already appears in the database.
        ///     CursorPutOptions.NoOverwrite - enter the new key/data pair only if the key does not already appear in the database.
        ///     The function will return MDB_KEYEXIST if the key already appears in the database, even if the database supports
        ///     duplicates (MDB_DUPSORT).
        ///     CursorPutOptions.ReserveSpace - reserve space for data of the given size, but don't copy the given data. Instead,
        ///     return a pointer to the reserved space, which the caller can fill in later. This saves an extra memcpy if the data
        ///     is being generated later.
        ///     CursorPutOptions.AppendData - append the given key/data pair to the end of the database. No key comparisons are
        ///     performed. This option allows fast bulk loading when keys are already known to be in the correct order. Loading
        ///     unsorted keys with this flag will cause data corruption.
        ///     CursorPutOptions.AppendDuplicateData - as above, but for sorted dup data.
        /// </param>
        /// <returns>Returns <see cref="MDBResultCode" /></returns>
        public unsafe MDBResultCode Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value, CursorPutOptions options) {
            fixed (byte* keyPtr = key)
            fixed (byte* valPtr = value) {
                var mdbKey = new MDBValue(key.Length, keyPtr);
                var mdbValue = new MDBValue(value.Length, valPtr);

                return mdb_cursor_put(_handle, mdbKey, mdbValue, options);
            }
        }

        /// <summary>
        ///     Delete current key/data pair.
        ///     This function deletes the key/data pair to which the cursor refers.
        /// </summary>
        /// <param name="option">
        ///     Options for this operation. This parameter must be set to 0 or one of the values described here.
        ///     MDB_NODUPDATA - delete all of the data items for the current key. This flag may only be specified if the database
        ///     was opened with MDB_DUPSORT.
        /// </param>
        private MDBResultCode Delete(CursorDeleteOption option) {
            return mdb_cursor_del(_handle, option);
        }

        /// <summary>
        ///     Delete current key/data pair.
        ///     This function deletes the key/data pair to which the cursor refers.
        /// </summary>
        public MDBResultCode Delete() {
            return Delete(CursorDeleteOption.None);
        }

        /// <summary>
        ///     Renew a cursor handle.
        ///     Cursors are associated with a specific transaction and database and may not span threads.
        ///     Cursors that are only used in read-only transactions may be re-used, to avoid unnecessary malloc/free overhead.
        ///     The cursor may be associated with a new read-only transaction, and referencing the same database handle as it was
        ///     created with.
        /// </summary>
        /// <returns>Returns <see cref="MDBResultCode" /></returns>
        public MDBResultCode Renew() {
            return Renew(Transaction);
        }

        /// <summary>
        ///     Renew a cursor handle.
        ///     Cursors are associated with a specific transaction and database and may not span threads.
        ///     Cursors that are only used in read-only transactions may be re-used, to avoid unnecessary malloc/free overhead.
        ///     The cursor may be associated with a new read-only transaction, and referencing the same database handle as it was
        ///     created with.
        /// </summary>
        /// <param name="txn">Transaction to renew in.</param>
        /// <returns>Returns <see cref="MDBResultCode" /></returns>
        public MDBResultCode Renew(LightningTransaction txn) {
            if (!txn.IsReadOnly) {
                throw new InvalidOperationException("Can't renew cursor on non-readonly transaction");
            }

            return mdb_cursor_renew(txn.Handle(), _handle);
        }

        /// <summary>
        ///     Closes the cursor and deallocates all resources associated with it.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        private void Dispose(bool disposing) {
            if (_handle == IntPtr.Zero) {
                return;
            }

            if (!disposing) {
                throw new InvalidOperationException("The LightningCursor was not disposed and cannot be reliably dealt with from the finalizer");
            }

            mdb_cursor_close(_handle);
            _handle = IntPtr.Zero;
        }

        /// <summary>
        ///     Closes the cursor and deallocates all resources associated with it.
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }
    }
}