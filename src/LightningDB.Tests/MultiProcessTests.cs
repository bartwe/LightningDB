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
            var testOutputDirectory = new DirectoryInfo(AppContext.BaseDirectory);
            var testBinDirectory = testOutputDirectory;
            while (testBinDirectory.Name != "bin") {
                testBinDirectory = testBinDirectory.Parent ?? throw new InvalidOperationException("Could not resolve the test build output root.");
            }
            var sourceDirectory = testBinDirectory.Parent?.Parent ?? throw new InvalidOperationException("Could not resolve the source directory.");
            var relativeOutputPath = Path.GetRelativePath(testBinDirectory.FullName, testOutputDirectory.FullName);
            var otherProcessProjectDirectory = Path.Combine(sourceDirectory.FullName, "SecondProcess");
            var otherProcessOutputDirectory = Path.Combine(otherProcessProjectDirectory, "bin", relativeOutputPath);
            var otherProcessFileName = OperatingSystem.IsWindows() ? "SecondProcess.exe" : "SecondProcess";
            var otherProcessPath = Path.Combine(otherProcessOutputDirectory, otherProcessFileName);
            using var process = new Process {
                StartInfo = new() {
                    FileName = otherProcessPath,
                    Arguments = $"{name}",
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                },
            };

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
