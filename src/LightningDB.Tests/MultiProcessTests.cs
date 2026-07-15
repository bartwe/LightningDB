using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;

namespace LightningDB.Tests {
    [Collection("SharedFileSystem")]
    public class MultiProcessTests {
        public MultiProcessTests(SharedFileSystem fileSystem) {
            _fileSystem = fileSystem;
        }

        readonly SharedFileSystem _fileSystem;

        [Fact] //(Skip = "Hangs on Linux only for some reason")]
        public void can_load_environment_from_multiple_processes() {
            var name = _fileSystem.CreateNewDirectoryForTest();
            using var env = new LightningEnvironment(name);
            env.Open();
            var executableName = OperatingSystem.IsWindows() ? "SecondProcess.exe" : "SecondProcess";
            var otherProcessPath = Path.Combine(AppContext.BaseDirectory, executableName);
            if (!File.Exists(otherProcessPath)) {
                throw new FileNotFoundException("The multi-process test helper was not built for the active configuration.", otherProcessPath);
            }
            using var process = new Process {
                StartInfo = new() {
                    FileName = otherProcessPath,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                },
            };
            process.StartInfo.ArgumentList.Add(name);

            var expected = "world";
            using var tx = env.BeginTransaction();
            using var db = tx.OpenDatabase();
            tx.Put(db, Encoding.UTF8.GetBytes("hello"), Encoding.UTF8.GetBytes(expected));
            tx.Commit();

            var current = Process.GetCurrentProcess();
            process.Start();
            Assert.NotEqual(current.Id, process.Id);

            var result = process.StandardOutput.ReadLine();
            process.WaitForExit();
            Assert.Equal(expected, result);
        }
    }
}
