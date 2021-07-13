﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB.Tests {
    public static class TestHelperExtensions {
        public static MDBResultCode Put(this LightningTransaction tx, LightningDatabase db, string key, string value) {
            var enc = Encoding.UTF8;
            return tx.Put(db, enc.GetBytes(key), enc.GetBytes(value));
        }

        public static string Get(this LightningTransaction tx, LightningDatabase db, string key) {
            var enc = Encoding.UTF8;
            var result = tx.Get(db, enc.GetBytes(key));
            return enc.GetString(result.value.AsSpan().ToArray());
        }

        public static void Delete(this LightningTransaction tx, LightningDatabase db, string key) {
            var enc = Encoding.UTF8;
            tx.Delete(db, enc.GetBytes(key));
        }

        public static bool ContainsKey(this LightningTransaction tx, LightningDatabase db, string key) {
            var enc = Encoding.UTF8;
            return tx.ContainsKey(db, enc.GetBytes(key));
        }

        public static bool TryGet(this LightningTransaction tx, LightningDatabase db, string key, out string value) {
            var enc = Encoding.UTF8;
            ReadOnlySpan<byte> result;
            var found = tx.TryGet(db, enc.GetBytes(key), out result);
            value = enc.GetString(result);
            return found;
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts) {
            return list.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / parts).Select(x => x.Select(v => v.Value));
        }

        public static void RunCursorScenario(this LightningEnvironment env, Action<LightningTransaction, LightningDatabase, LightningCursor> scenario, DatabaseOpenFlags flags = DatabaseOpenFlags.Create, TransactionBeginFlags transactionFlags = TransactionBeginFlags.None) {
            using var tx = env.BeginTransaction(transactionFlags);
            using var db = tx.OpenDatabase(configuration: new() { Flags = flags });
            using var cursor = tx.CreateCursor(db);
            scenario(tx, db, cursor);
        }

        public static void RunTransactionScenario(this LightningEnvironment env, Action<LightningTransaction, LightningDatabase> scenario, DatabaseOpenFlags flags = DatabaseOpenFlags.Create, TransactionBeginFlags transactionFlags = TransactionBeginFlags.None) {
            using var tx = env.BeginTransaction(transactionFlags);
            using var db = tx.OpenDatabase(configuration: new() { Flags = flags });
            scenario(tx, db);
        }
    }
}
