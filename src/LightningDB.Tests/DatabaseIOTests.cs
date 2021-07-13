using System;
using Xunit;
using static System.Text.Encoding;

namespace LightningDB.Tests {
    [Collection("SharedFileSystem")]
    public class DatabaseIOTests : IDisposable {
        public DatabaseIOTests(SharedFileSystem fileSystem) {
            var path = fileSystem.CreateNewDirectoryForTest();

            _env = new(path);
            _env.MaxDatabases = 2;
            _env.Open();

            _txn = _env.BeginTransaction();
            _db = _txn.OpenDatabase(configuration: new() { Flags = DatabaseOpenFlags.Create });
        }

        public void Dispose() {
            _env.Dispose();
        }

        readonly LightningEnvironment _env;
        LightningTransaction _txn;
        readonly LightningDatabase _db;

        [Fact]
        public void CanCommitTransactionToNamedDatabase() {
            using (var db = _txn.OpenDatabase("test", new() { Flags = DatabaseOpenFlags.Create })) {
                _txn.Put(db, "key1", "value");

                _txn.Commit();
            }

            using (var txn2 = _env.BeginTransaction()) {
                using (var db = txn2.OpenDatabase("test")) {
                    var value = txn2.Get(db, "key1");
                    Assert.Equal("value", value);
                }
            }
        }

        [Fact]
        public void ContainsKeyShouldReturnFalseIfKeyNotExists() {
            var key = "key";

            var exists = _txn.ContainsKey(_db, key);

            Assert.False(exists);
        }

        [Fact]
        public void ContainsKeyShouldReturnTrueIfKeyExists() {
            var key = "key";
            var value = "value";

            _txn.Put(_db, key, value);

            var exists = _txn.ContainsKey(_db, key);

            Assert.True(exists);
        }

        [Fact]
        public void DatabaseDeleteShouldRemoveItem() {
            var key = "key";
            var value = "value";
            _txn.Put(_db, key, value);

            _txn.Delete(_db, key);

            Assert.False(_txn.ContainsKey(_db, key));
        }

        [Fact]
        public void DatabaseGetShouldNotThrowExceptions() {
            _txn.Get(_db, UTF8.GetBytes("key"));
        }

        [Fact]
        public void DatabaseInsertedValueShouldBeRetrievedThen() {
            var key = "key";
            var value = "value";
            _txn.Put(_db, key, value);

            var persistedValue = _txn.Get(_db, key);

            Assert.Equal(persistedValue, value);
        }

        [Fact]
        public void DatabasePutShouldNotThrowExceptions() {
            var key = "key";
            var value = "value";

            _txn.Put(_db, key, value);
        }

        [Fact]
        public void TryGetShouldReturnValueIfKeyExists() {
            var key = "key";
            var value = "value";

            _txn.Put(_db, key, value);

            string persistedValue;
            var exists = _txn.TryGet(_db, key, out persistedValue);

            Assert.True(exists);
            Assert.Equal(value, persistedValue);
        }
    }
}
