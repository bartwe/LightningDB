using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LightningDB.Native;

namespace LightningDB;

public static class LightningExtensions {
    /// <summary>
    ///     Throws a <see cref="LightningException" /> on anything other than NotFound, or Success
    /// </summary>
    /// <param name="resultCode">The result code to evaluate for errors</param>
    /// <returns>
    ///     <see cref="MDBResultCode" />
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowOnReadError(this MDBResultCode resultCode) {
        if (resultCode == MDBResultCode.NotFound) {
            return;
        }
        resultCode.ThrowOnError();
    }

    /// <summary>
    ///     Throws a <see cref="LightningException" /> on anything other than Success
    /// </summary>
    /// <param name="resultCode">The result code to evaluate for errors</param>
    /// <returns>
    ///     <see cref="MDBResultCode" />
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowOnError(this MDBResultCode resultCode) {
        if (resultCode == MDBResultCode.Success) {
            return;
        }
        var statusCode = (int)resultCode;
        var message = mdb_strerror(statusCode);
        throw new LightningException(message, statusCode);
    }

    /// <summary>
    ///     Throws a <see cref="LightningException" /> on anything other than NotFound, or Success
    /// </summary>
    /// <param name="result">A <see cref="ValueTuple" /> representing the get result operation</param>
    /// <returns>The provided <see cref="ValueTuple" /> if no error occurs</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (MDBResultCode resultCode, MDBValue key, MDBValue value) ThrowOnReadError(this ValueTuple<MDBResultCode, MDBValue, MDBValue> result) {
        result.Item1.ThrowOnReadError();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string mdb_strerror(int err) {
        var ptr = Lmdb.mdb_strerror(err);
        var result = Marshal.PtrToStringUTF8(ptr);
        if (result == null)
            result = new Win32Exception(err).Message;
        return result;
    }

    /// <summary>
    ///     Enumerates the key/value pairs of the <see cref="LightningCursor" /> starting at the current position.
    /// </summary>
    /// <param name="cursor">
    ///     <see cref="LightningCursor" />
    /// </param>
    /// <returns><see cref="ValueTuple" /> key/value pairs of <see cref="MDBValue" /></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<ValueTuple<MDBValue, MDBValue>> AsEnumerable(this LightningCursor cursor) {
        while (cursor.Next() == MDBResultCode.Success) {
            var (resultCode, key, value) = cursor.GetCurrent();
            resultCode.ThrowOnError();
            yield return (key, value);
        }
    }

    /// <summary>
    ///     Tries to get a value by its key.
    /// </summary>
    /// <param name="tx">The transaction.</param>
    /// <param name="db">The database to query.</param>
    /// <param name="key">A span containing the key to look up.</param>
    /// <param name="value">A byte array containing the value found in the database, if it exists.</param>
    /// <returns>True if key exists, false if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGet(this LightningTransaction tx, LightningDatabase db, ReadOnlySpan<byte> key, out ReadOnlySpan<byte> value) {
        var (resultCode, mdbValue) = tx.Get(db, key);
        if (resultCode == MDBResultCode.Success) {
            value = mdbValue.AsReadonlySpan();
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    ///     Check whether data exists in database.
    /// </summary>
    /// <param name="tx">The transaction.</param>
    /// <param name="db">The database to query.</param>
    /// <param name="key">A span containing the key to look up.</param>
    /// <returns>True if key exists, false if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsKey(this LightningTransaction tx, LightningDatabase db, ReadOnlySpan<byte> key) {
        var (resultCode, _) = tx.Get(db, key);
        return resultCode == MDBResultCode.Success;
    }
}
