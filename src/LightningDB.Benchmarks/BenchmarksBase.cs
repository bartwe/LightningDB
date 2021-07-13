using System;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace LightningDB.Benchmarks {
    public abstract class BenchmarksBase {
        public LightningEnvironment Env { get; set; }
        public LightningDatabase DB { get; set; }

        [GlobalSetup]
        public void GlobalSetup() {
            Console.WriteLine("Global Setup Begin");

            const string Path = "TestDirectory";

            if (Directory.Exists(Path))
                Directory.Delete(Path, true);

            Env = new(Path) { MaxDatabases = 1 };

            Env.Open();

            using (var tx = Env.BeginTransaction()) {
                DB = tx.OpenDatabase();
                tx.Commit();
            }

            RunSetup();

            Console.WriteLine("Global Setup End");
        }

        public abstract void RunSetup();

        [GlobalCleanup]
        public void GlobalCleanup() {
            Console.WriteLine("Global Cleanup Begin");

            try {
                DB.Dispose();
                Env.Dispose();
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Global Cleanup End");
        }
    }
}
