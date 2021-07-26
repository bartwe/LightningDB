﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LightningDB.Native;
using static LightningDB.Native.Lmdb;

namespace LightningDB {
    public sealed class DatabaseConfiguration {
        public static DatabaseConfiguration Default = new();

        private IComparer<MDBValue>? _comparer;

        public DatabaseConfiguration() {
            Flags = DatabaseOpenFlags.None;
        }

        public DatabaseOpenFlags Flags { get; set; }


        internal IDisposable ConfigureDatabase(LightningTransaction tx, LightningDatabase db) {
            var pinnedComparer = new ComparerKeepAlive();
            if (_comparer != null) {
                CompareFunction compare = Compare;
                pinnedComparer.AddComparer(compare);
                mdb_set_compare(tx.Handle(), db.Handle(), compare);
            }
            return pinnedComparer;
        }

        private int Compare(ref MDBValue left, ref MDBValue right) {
            return _comparer!.Compare(left, right);
        }

        public void CompareWith(IComparer<MDBValue> comparer) {
            _comparer = comparer;
        }

        private sealed class ComparerKeepAlive : IDisposable {
            private readonly List<GCHandle> _comparisons = new();

            public void Dispose() {
                for (var i = 0; i < _comparisons.Count; ++i) {
                    _comparisons[i].Free();
                }
            }

            public void AddComparer(CompareFunction compare) {
                var handle = GCHandle.Alloc(compare);
                _comparisons.Add(handle);
            }
        }
    }
}