﻿namespace LightningDB {
    /// <summary>
    ///     Basic environment configuration
    /// </summary>
    public sealed class EnvironmentConfiguration {
        private long? _mapSize;
        private int? _maxReaders;
        private int? _maxDatabases;

        private bool? _autoResizeWindows;

        public long MapSize {
            get => _mapSize ?? 0;
            set => _mapSize = value;
        }

        public int MaxReaders {
            get => _maxReaders ?? 0;
            set => _maxReaders = value;
        }

        public int MaxDatabases {
            get => _maxDatabases ?? 0;
            set => _maxDatabases = value;
        }

        public bool AutoReduceMapSizeIn32BitProcess { get; set; }

        public bool AutoResizeWindows {
            get => _autoResizeWindows ?? false;
            set => _autoResizeWindows = value;
        }

        internal void Configure(LightningEnvironment env) {
            if (_mapSize.HasValue) {
                env.MapSize = _mapSize.Value;
            }

            if (_maxDatabases.HasValue) {
                env.MaxDatabases = _maxDatabases.Value;
            }

            if (_maxReaders.HasValue) {
                env.MaxReaders = _maxReaders.Value;
            }
        }
    }
}