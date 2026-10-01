using System;
using System.Linq;
using LightningDB.Benchmarks;
using Xunit;

namespace LightningDB.Tests {
    public class KeyBatchTests {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(256)]
        [InlineData(257)]
        [InlineData(1000)]
        [InlineData(65537)]
        public void SequentialKeysAreStrictlyIncreasingInByteOrder(int keyCount) {
            var batch = KeyBatch.Generate(keyCount, KeyOrdering.Sequential);

            Assert.Equal(keyCount, batch.Count);
            for (var i = 0; i < batch.Count; i++) {
                Assert.Equal(4, batch[i].Length);
                if (i > 0) {
                    Assert.True(batch[i - 1].AsSpan().SequenceCompareTo(batch[i]) < 0,
                        $"Keys {i - 1} ({Convert.ToHexString(batch[i - 1])}) and {i} ({Convert.ToHexString(batch[i])}) are not increasing.");
                }
            }
        }

        [Fact]
        public void RandomKeysAreADeterministicPermutationOfSequentialKeys() {
            var sequential = KeyBatch.Generate(1000, KeyOrdering.Sequential).Buffers.Select(key => Convert.ToHexString(key)).ToArray();
            var random = KeyBatch.Generate(1000, KeyOrdering.Random).Buffers.Select(key => Convert.ToHexString(key)).ToArray();
            var repeated = KeyBatch.Generate(1000, KeyOrdering.Random).Buffers.Select(key => Convert.ToHexString(key)).ToArray();

            Assert.Equal(sequential.OrderBy(key => key, StringComparer.Ordinal), random.OrderBy(key => key, StringComparer.Ordinal));
            Assert.Equal(random, repeated);
            Assert.False(sequential.SequenceEqual(random));
        }
    }
}
