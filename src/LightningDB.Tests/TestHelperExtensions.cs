using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightningDB.Tests {
    public static class TestHelperExtensions {
        public delegate void CursorScenario(ref LightningTransaction transaction, LightningDatabase database, ref LightningCursor cursor);

        public delegate void TransactionScenario(ref LightningTransaction transaction, LightningDatabase database);

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

        public static void RunCursorScenario(this LightningEnvironment env, CursorScenario scenario, DatabaseOpenFlags flags = DatabaseOpenFlags.Create, TransactionBeginFlags transactionFlags = TransactionBeginFlags.None) {
            var transaction = env.BeginTransaction(transactionFlags);
            try {
                using var database = transaction.OpenDatabase(configuration: new() { Flags = flags });
                var cursor = transaction.CreateCursor(database);
                try {
                    scenario(ref transaction, database, ref cursor);
                }
                finally {
                    cursor.Dispose();
                }
            }
            finally {
                transaction.Dispose();
            }
        }

        public static void RunTransactionScenario(this LightningEnvironment env, TransactionScenario scenario, DatabaseOpenFlags flags = DatabaseOpenFlags.Create, TransactionBeginFlags transactionFlags = TransactionBeginFlags.None) {
            var transaction = env.BeginTransaction(transactionFlags);
            try {
                using var database = transaction.OpenDatabase(configuration: new() { Flags = flags });
                scenario(ref transaction, database);
            }
            finally {
                transaction.Dispose();
            }
        }
    }
}
