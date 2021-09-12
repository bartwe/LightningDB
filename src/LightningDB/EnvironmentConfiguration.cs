namespace LightningDB {
    /// <summary>
    ///     Basic environment configuration
    /// </summary>
    public sealed class EnvironmentConfiguration {
        long? _mapSize;
        int? _maxReaders;
        int? _maxDatabases;

        bool? _autoResizeWindows;

        public long MapSize {
            get { return _mapSize ?? 0; }
            set { _mapSize = value; }
        }

        public int MaxReaders {
            get { return _maxReaders ?? 0; }
            set { _maxReaders = value; }
        }

        public int MaxDatabases {
            get { return _maxDatabases ?? 0; }
            set { _maxDatabases = value; }
        }

        public bool AutoReduceMapSizeIn32BitProcess { get; set; }

        public bool AutoResizeWindows {
            get { return _autoResizeWindows ?? false; }
            set { _autoResizeWindows = value; }
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
