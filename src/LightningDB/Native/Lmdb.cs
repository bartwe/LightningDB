using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LightningDB.Native;

static class Lmdb {
    const string MDB_DLL_NAME = "lmdb";

    public static MDBResultCode mdb_env_set_mapsize(IntPtr env, long size) {
        return mdb_env_set_mapsize(env, new IntPtr(size));
    }

    public static MDBResultCode mdb_put(IntPtr txn, uint dbi, MDBValue key, ref MDBValue value, PutOptions flags) {
        return mdb_put(txn, dbi, ref key, ref value, flags);
    }

    public static MDBResultCode mdb_del(IntPtr txn, uint dbi, MDBValue key, MDBValue value) {
        return mdb_del(txn, dbi, ref key, ref value);
    }

    public static MDBResultCode mdb_del(IntPtr txn, uint dbi, MDBValue key) {
        return mdb_del(txn, dbi, ref key, IntPtr.Zero);
    }

    public static MDBResultCode mdb_cursor_put(IntPtr cursor, MDBValue key, MDBValue value, CursorPutOptions flags) {
        return mdb_cursor_put(cursor, ref key, ref value, flags);
    }

    /// <summary>
    ///     store multiple contiguous data elements in a single request.
    ///     May only be used with MDB_DUPFIXED.
    /// </summary>
    /// <param name="data">This span must be pinned or stackalloc memory</param>
    public static MDBResultCode mdb_cursor_put(IntPtr cursor, ref MDBValue key, ref Span<MDBValue> data, CursorPutOptions flags) {
        ref var dataRef = ref MemoryMarshal.GetReference(data);
        return mdb_cursor_put(cursor, ref key, ref dataRef, flags);
    }

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_env_create(out IntPtr env);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void mdb_env_close(IntPtr env);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    internal static extern MDBResultCode mdb_env_open(IntPtr env, IntPtr path, EnvironmentOpenFlags flags, UnixAccessMode mode);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_env_set_mapsize(IntPtr env, IntPtr size);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_env_get_maxreaders(IntPtr env, out uint readers);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_env_set_maxreaders(IntPtr env, uint readers);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_env_set_maxdbs(IntPtr env, uint dbs);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_dbi_open(IntPtr txn, IntPtr name, DatabaseOpenFlags flags, out uint db);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void mdb_dbi_close(IntPtr env, uint dbi);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_drop(IntPtr txn, uint dbi, bool del);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_txn_begin(IntPtr env, IntPtr parent, TransactionBeginFlags flags, out IntPtr txn);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_txn_commit(IntPtr txn);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void mdb_txn_abort(IntPtr txn);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void mdb_txn_reset(IntPtr txn);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_txn_renew(IntPtr txn);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr mdb_version(out int major, out int minor, out int patch);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr mdb_strerror(int err);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_stat(IntPtr txn, uint dbi, out MDBStat stat);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_env_copy(IntPtr env, IntPtr path);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_env_copy2(IntPtr env, IntPtr path, EnvironmentCopyFlags copyFlags);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_env_info(IntPtr env, out MDBEnvInfo stat);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_env_stat(IntPtr env, out MDBStat stat);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_env_sync(IntPtr env, bool force);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_get(IntPtr txn, uint dbi, ref MDBValue key, out MDBValue data);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_put(IntPtr txn, uint dbi, ref MDBValue key, ref MDBValue data, PutOptions flags);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_del(IntPtr txn, uint dbi, ref MDBValue key, ref MDBValue data);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_del(IntPtr txn, uint dbi, ref MDBValue key, IntPtr data);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_cursor_open(IntPtr txn, uint dbi, out IntPtr cursor);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void mdb_cursor_close(IntPtr cursor);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_cursor_renew(IntPtr txn, IntPtr cursor);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_cursor_get(IntPtr cursor, ref MDBValue key, ref MDBValue data, CursorOperation op);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_cursor_put(IntPtr cursor, ref MDBValue key, ref MDBValue mdbValue, CursorPutOptions flags);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_cursor_del(IntPtr cursor, CursorDeleteOption flags);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_set_compare(IntPtr txn, uint dbi, CompareFunction cmp);

    [DllImport(MDB_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern MDBResultCode mdb_set_dupsort(IntPtr txn, uint dbi, CompareFunction cmp);

    static bool _ShouldSetDllImportResolver = true;
    static readonly object _SyncRoot = new();

    public static void LoadWindowsAutoResizeLibrary() {
        if (_ShouldSetDllImportResolver) {
            lock (_SyncRoot) {
                if (_ShouldSetDllImportResolver) {
                    NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
                    _ShouldSetDllImportResolver = false;
                }
            }
        }
    }

    static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) {
        if (libraryName == MDB_DLL_NAME) {
            return NativeLibrary.Load($"{MDB_DLL_NAME}autoresize", assembly, searchPath);
        }
        return IntPtr.Zero;
    }
}
