using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LightningDB.Tests {
    [Collection("SharedFileSystem")]
    public class TransactionTests : IDisposable {
        public TransactionTests(SharedFileSystem fileSystem) {
            var path = fileSystem.CreateNewDirectoryForTest();
            _env = new(path);
            _env.Open();
        }

        public void Dispose() {
            _env.Dispose();
        }

        readonly LightningEnvironment _env;

        [Fact]
        public void CanCountTransactionEntries() {
            _env.RunTransactionScenario(
                (tx, db) => {
                    const int entriesCount = 10;
                    for (var i = 0; i < entriesCount; i++)
                        tx.Put(db, i.ToString(), i.ToString());

                    var count = tx.GetEntriesCount(db);
                    Assert.Equal(entriesCount, count);
                }
            );
        }

        [Fact]
        public void ReadOnlyTransactionShouldChangeStateOnRenew() {
            _env.RunTransactionScenario(
                (tx, db) => {
                    tx.Reset();
                    tx.Renew();
                    Assert.Equal(LightningTransactionState.Active, tx.State);
                }, transactionFlags: TransactionBeginFlags.ReadOnly
            );
        }

        [Fact]
        public void ReadOnlyTransactionShouldChangeStateOnReset() {
            _env.RunTransactionScenario(
                (tx, db) => {
                    tx.Reset();
                    Assert.Equal(LightningTransactionState.Reseted, tx.State);
                }, transactionFlags: TransactionBeginFlags.ReadOnly
            );
        }

        [Fact]
        public void ResetTransactionAbortedOnDispose() {
            _env.RunTransactionScenario(
                (tx, db) => {
                    tx.Reset();
                    tx.Dispose();
                    Assert.Equal(LightningTransactionState.Aborted, tx.State);
                }, transactionFlags: TransactionBeginFlags.ReadOnly
            );
        }

        [Fact]
        public void TransactionShouldBeAbortedIfEnvironmentCloses() {
            _env.RunTransactionScenario(
                (tx, db) => {
                    _env.Dispose();
                    Assert.Equal(LightningTransactionState.Aborted, tx.State);
                }
            );
        }

        [Fact]
        public void TransactionShouldBeCreated() {
            _env.RunTransactionScenario((tx, db) => { Assert.Equal(LightningTransactionState.Active, tx.State); });
        }

        [Fact]
        public void TransactionShouldChangeStateOnCommit() {
            _env.RunTransactionScenario(
                (tx, db) => {
                    tx.Commit();
                    Assert.Equal(LightningTransactionState.Commited, tx.State);
                }
            );
        }

        [Fact]
        public void TransactionShouldSupportCustomComparer() {
            Func<int, int, int> comparison = (l, r) => l.CompareTo(r);
            var options = new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create };
            Func<MDBValue, MDBValue, int> compareWith = (l, r) => comparison(BitConverter.ToInt32(l.AsSpan().ToArray(), 0), BitConverter.ToInt32(r.AsSpan().ToArray(), 0));
            options.CompareWith(Comparer<MDBValue>.Create(new(compareWith)));

            using (var txnT = _env.BeginTransaction())
            using (var db1 = txnT.OpenDatabase(configuration: options)) {
                txnT.DropDatabase(db1);
                txnT.Commit();
            }

            var txn = _env.BeginTransaction();
            var db = txn.OpenDatabase(configuration: options);

            var keysUnsorted = Enumerable.Range(1, 10000).OrderBy(x => Guid.NewGuid()).ToList();
            var keysSorted = keysUnsorted.ToArray();
            Array.Sort(keysSorted, new Comparison<int>(comparison));

            GC.Collect();
            for (var i = 0; i < keysUnsorted.Count; i++)
                txn.Put(db, BitConverter.GetBytes(keysUnsorted[i]), BitConverter.GetBytes(i));

            using (var c = txn.CreateCursor(db)) {
                var order = 0;
                while (c.Next() == MDBResultCode.Success)
                    Assert.Equal(keysSorted[order++], BitConverter.ToInt32(c.GetCurrent().key.AsSpan().ToArray(), 0));
            }
        }
    }
}
