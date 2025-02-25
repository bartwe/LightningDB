﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LightningDB.Benchmarks {
    /// <summary>
    ///     A collection of 4 byte key arrays
    /// </summary>
    public class KeyBatch {
        KeyBatch(byte[][] buffers) {
            Buffers = buffers;
        }

        public byte[][] Buffers { get; }


        public int Count {
            get { return Buffers.Length; }
        }

        public ref byte[] this[int index] {
            get { return ref Buffers[index]; }
        }


        public static KeyBatch Generate(int keyCount, KeyOrdering keyOrdering) {
            var buffers = new byte[keyCount][];

            switch (keyOrdering) {
                case KeyOrdering.Sequential:
                    PopulateSequential(buffers);
                    break;

                case KeyOrdering.Random:
                    PopulateRandom(buffers);
                    break;

                default:
                    throw new ArgumentException("That isn't a valid KeyOrdering", nameof(keyOrdering));
            }

            return new(buffers);
        }

        static void PopulateSequential(byte[][] buffers) {
            for (var i = 0; i < buffers.Length; i++)
                buffers[i] = CopyToArray(i);
        }

        static void PopulateRandom(byte[][] buffers) {
            var random = new Random(0);
            var seen = new HashSet<int>(buffers.Length);

            var i = 0;
            while (i < buffers.Length) {
                var keyValue = random.Next(0, buffers.Length);

                if (!seen.Add(keyValue))
                    continue; //skip duplicates

                buffers[i++] = CopyToArray(keyValue);
            }
        }

        static byte[] CopyToArray(int keyValue) {
            var key = new byte[4];
            MemoryMarshal.Write(key, ref keyValue);
            return key;
        }
    }
}
