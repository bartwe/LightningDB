using System;
using System.Linq;
using Xunit;
using static System.Text.Encoding;

namespace LightningDB.Tests {
    [Collection("SharedFileSystem")]
    public class CursorTests : IDisposable {
        public CursorTests(SharedFileSystem fileSystem) {
            var path = fileSystem.CreateNewDirectoryForTest();
            _env = new(path);
            _env.Open();
        }

        public void Dispose() {
            _env.Dispose();
        }

        readonly LightningEnvironment _env;

        static byte[][] PopulateCursorValues(LightningCursor cursor, int count = 5, string keyPrefix = "key") {
            var keys = Enumerable.Range(1, count).Select(i => UTF8.GetBytes(keyPrefix + i)).ToArray();

            foreach (var k in keys) {
                var result = cursor.Put(k, k, CursorPutOptions.None);
                Assert.Equal(MDBResultCode.Success, result);
            }

            return keys;
        }

        [Fact]
        public void CursorShouldDeleteElements() {
            _env.RunCursorScenario(
                (tx, db, c) => {
                    var keys = PopulateCursorValues(c).Take(2).ToArray();
                    for (var i = 0; i < 2; ++i) {
                        c.Next();
                        c.Delete();
                    }

                    using var c2 = tx.CreateCursor(db);
                    Assert.DoesNotContain(c2.AsEnumerable(), x => keys.Any(k => x.Item1.AsSpan().ToArray() == k));
                }
            );
        }

        [Fact]
        public void CursorShouldMoveToFirst() {
            _env.RunCursorScenario(
                (tx, db, c) => {
                    var keys = PopulateCursorValues(c);
                    var firstKey = keys.First();
                    var result = c.First();
                    Assert.Equal(MDBResultCode.Success, result);
                    var current = c.GetCurrent();
                    Assert.Equal(MDBResultCode.Success, current.resultCode);
                    Assert.Equal(firstKey, current.key.AsSpan().ToArray());
                }
            );
        }

        [Fact]
        public void CursorShouldMoveToLast() {
            _env.RunCursorScenario(
                (tx, db, c) => {
                    var keys = PopulateCursorValues(c);
                    var lastKey = keys.Last();
                    var result = c.Last();
                    Assert.Equal(MDBResultCode.Success, result);
                    var current = c.GetCurrent();
                    Assert.Equal(MDBResultCode.Success, current.resultCode);
                    Assert.Equal(lastKey, current.key.AsSpan().ToArray());
                }
            );
        }

        [Fact]
        public void CursorShouldPutValues() {
            _env.RunCursorScenario(
                (tx, db, c) => {
                    PopulateCursorValues(c);
                    c.Dispose();
                    //TODO evaluate how not to require this Dispose on Linux (test only fails there)
                    var result = tx.Commit();
                    Assert.Equal(MDBResultCode.Success, result);
                }
            );
        }

        [Fact]
        public void CursorShouldSetSpanKey() {
            _env.RunCursorScenario(
                (tx, db, c) => {
                    var keys = PopulateCursorValues(c);
                    var firstKey = keys.First();
                    var result = c.Set(firstKey.AsSpan());
                    Assert.Equal(MDBResultCode.Success, result);
                    var current = c.GetCurrent();
                    Assert.Equal(MDBResultCode.Success, current.resultCode);
                    Assert.Equal(firstKey, current.key.AsSpan().ToArray());
                }
            );
        }

        [Fact]
        public void ShouldAdvanceKeyToClosestWhenKeyNotFound() {
            _env.RunCursorScenario(
                (tx, db, c) => {
                    var expected = PopulateCursorValues(c).First();
                    var result = c.Set(UTF8.GetBytes("key"));
                    Assert.Equal(MDBResultCode.NotFound, result);
                    var current = c.GetCurrent();
                    Assert.Equal(MDBResultCode.Success, current.resultCode);
                    Assert.Equal(expected, current.key.AsSpan().ToArray());
                }
            );
        }

        [Fact]
        public void ShouldIterateThroughCursor() {
            _env.RunCursorScenario(
                (tx, db, c) => {
                    var keys = PopulateCursorValues(c);
                    using var c2 = tx.CreateCursor(db);
                    var items = c2.AsEnumerable().Select((x, i) => (x, i)).ToList();
                    foreach (var (x, i) in items)
                        Assert.Equal(keys[i], x.Item1.AsSpan().ToArray());

                    Assert.Equal(keys.Length, items.Count);
                }
            );
        }

        [Fact]
        public void ShouldRenewSameTransaction() {
            _env.RunCursorScenario(
                (tx, db, c) => {
                    var result = c.Renew();
                    Assert.Equal(MDBResultCode.Success, result);
                }, transactionFlags: TransactionBeginFlags.ReadOnly
            );
        }

        [Fact]
        public void ShouldSetKeyAndGet() {
            _env.RunCursorScenario(
                (tx, db, c) => {
                    var expected = PopulateCursorValues(c).ElementAt(2);
                    var result = c.SetKey(expected);
                    Assert.Equal(MDBResultCode.Success, result.resultCode);
                    Assert.Equal(expected, result.key.AsSpan().ToArray());
                }
            );
        }

        [Fact]
        public void ShouldSetKeyAndGetWithSpan() {
            _env.RunCursorScenario(
                (tx, db, c) => {
                    var expected = PopulateCursorValues(c).ElementAt(2);
                    var result = c.SetKey(expected.AsSpan());
                    Assert.Equal(MDBResultCode.Success, result.resultCode);
                    Assert.Equal(expected, result.key.AsSpan().ToArray());
                }
            );
        }

        [Fact]
        public void ShouldSetRangeWithSpan() {
            _env.RunCursorScenario(
                (tx, db, c) => {
                    var values = PopulateCursorValues(c);
                    var firstAfter = values[0].AsSpan();
                    var result = c.SetRange(firstAfter);
                    Assert.Equal(MDBResultCode.Success, result);
                    var current = c.GetCurrent();
                    Assert.Equal(MDBResultCode.Success, current.resultCode);
                    Assert.Equal(values[0], current.value.AsSpan().ToArray());
                }
            );
        }
    }
}
