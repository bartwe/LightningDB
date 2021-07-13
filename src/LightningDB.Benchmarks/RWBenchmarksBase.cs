using BenchmarkDotNet.Attributes;

namespace LightningDB.Benchmarks {
    public abstract class RWBenchmarksBase : BenchmarksBase {
        //***** Argument Matrix Start *****//
        [Params(1, 100, 1000)] public int OpsPerTransaction { get; set; }

        [Params(8, 64, 256)] public int ValueSize { get; set; }

        [Params(KeyOrdering.Sequential)] public KeyOrdering KeyOrder { get; set; }

        //***** Argument Matrix End *****//


        //***** Test Values Begin *****//

        protected byte[] ValueBuffer { get; private set; }
        protected KeyBatch KeyBuffers { get; private set; }

        //***** Test Values End *****//

        public override void RunSetup() {
            ValueBuffer = new byte[ValueSize];
            KeyBuffers = KeyBatch.Generate(OpsPerTransaction, KeyOrder);
        }
    }
}
