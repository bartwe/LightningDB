﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace LightningDB.Tests {
    [Collection("SharedFileSystem")]
    public class EnvironmentTests : IDisposable {
        public EnvironmentTests(SharedFileSystem fileSystem) {
            _path = fileSystem.CreateNewDirectoryForTest();
            _pathCopy = fileSystem.CreateNewDirectoryForTest();
        }

        public void Dispose() {
            if (_env != null)
                _env.Dispose();

            _env = null;
        }

        readonly string _path;
        readonly string _pathCopy;
        LightningEnvironment _env;

        [Theory]
        [InlineData(1024 * 1024 * 200)]
        [InlineData(1024 * 1024 * 1024 * 3L)]
        public void CanGetEnvironmentInfo(long mapSize) {
            _env = new(_path, new() { MapSize = mapSize });
            _env.Open();
            var info = _env.Info;
            Assert.Equal(_env.MapSize, info.MapSize);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnvironmentShouldBeCopied(bool compact) {
            _env = new(_path);
            _env.Open();

            _env.CopyTo(_pathCopy, compact);

            if (Directory.GetFiles(_pathCopy).Length == 0)
                Assert.True(false, "Copied files doesn't exist");
        }

        [Fact]
        public void CanLoadAndDisposeMultipleEnvironments() {
            _env = new(_path);
            _env.Dispose();
            _env = new(_path);
        }

        [Fact]
        public void CanOpenEnvironmentMoreThan50Mb() {
            _env = new(_path) { MapSize = 55 * 1024 * 1024 };

            _env.Open();
        }

        [Fact(Skip = "Run manually, behavior will override all tests with auto resize")]
        public void CreateEnvironmentWithAutoResize() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                using (var env = new LightningEnvironment(_path, new() { MapSize = 10 * 1024 * 1024, AutoResizeWindows = true }))
                    env.Open();

                using (var dbFile = File.OpenRead(Path.Combine(_path, "data.mdb")))
                    Assert.Equal(8192, dbFile.Length);
            }
        }

        [Fact]
        public void EnvironmentCreatedFromConfig() {
            var mapExpected = 1024 * 1024 * 20;
            var maxDatabaseExpected = 2;
            var maxReadersExpected = 3;
            var config = new EnvironmentConfiguration { MapSize = mapExpected, MaxDatabases = maxDatabaseExpected, MaxReaders = maxReadersExpected };
            _env = new(_path, config);
            Assert.Equal(_env.MapSize, mapExpected);
            Assert.Equal(_env.MaxDatabases, maxDatabaseExpected);
            Assert.Equal(_env.MaxReaders, maxReadersExpected);
        }

        [Fact]
        public void EnvironmentShouldBeClosed() {
            _env = new(_path);
            _env.Open();

            _env.Dispose();

            Assert.False(_env.IsOpened);
        }

        [Fact]
        public void EnvironmentShouldBeCreatedIfReadOnly() {
            _env = new(_path);
            _env.Open(); //readonly requires environment to have been created at least once before
            _env.Dispose();
            _env = new(_path);
            _env.Open(EnvironmentOpenFlags.ReadOnly);
        }

        [Fact]
        public void EnvironmentShouldBeCreatedIfWithoutFlags() {
            _env = new(_path);
            _env.Open();
        }

        [Fact]
        public void EnvironmentShouldBeOpened() {
            _env = new(_path);
            _env.Open();

            Assert.True(_env.IsOpened);
        }

        [Fact]
        public void MaxDatabasesWorksThroughConfigIssue62() {
            var config = new EnvironmentConfiguration { MaxDatabases = 2 };
            _env = new(_path, config);
            _env.Open();
            using (var tx = _env.BeginTransaction()) {
                tx.OpenDatabase("db1", new() { Flags = DatabaseOpenFlags.Create });
                tx.OpenDatabase("db2", new() { Flags = DatabaseOpenFlags.Create });
                tx.Commit();
            }
            Assert.Equal(2, _env.MaxDatabases);
        }

        [Fact]
        public void StartingTransactionBeforeEnvironmentOpen() {
            _env = new(_path);
            Assert.Throws<InvalidOperationException>(() => _env.BeginTransaction());
        }
    }
}
