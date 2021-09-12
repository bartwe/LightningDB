﻿using System;

namespace LightningDB {
    /// <summary>
    ///     A managed version of the native MDB_val type
    /// </summary>
    /// <remarks>
    ///     For Performance and Correctness, the layout of this struct must not be changed.
    ///     This struct is blittable and is marshalled directly to Native code via
    ///     P/Invoke.
    /// </remarks>
    public unsafe struct MDBValue {
        /// <remarks>
        ///     We only expose this shape constructor to basically force you to use
        ///     a fixed statement to obtain the pointer. If we accepted a Span or
        ///     ReadOnlySpan here, we would have to do scarier things to pin/unpin
        ///     the buffer. Since this library is geared towards safe and easy usage,
        ///     this way somewhat forces you onto the correct path.
        /// </remarks>
        /// <param name="bufferSize">The length of the buffer</param>
        /// <param name="pinnedOrStackAllocBuffer">
        ///     A pointer to a buffer.
        ///     The underlying memory may be managed(an array), unmanaged or stack-allocated.
        ///     If it is managed, it **MUST** be pinned via either GCHandle.Alloc or a fixed statement
        /// </param>
        internal MDBValue(int bufferSize, byte* pinnedOrStackAllocBuffer) {
            size = (IntPtr)bufferSize;
            data = pinnedOrStackAllocBuffer;
        }

        //DO NOT REORDER
        internal IntPtr size;

        //DO NOT REORDER
        internal byte* data;

        /// <summary>
        ///     Gets a span representation of the buffer
        /// </summary>
        public ReadOnlySpan<byte> AsSpan() {
            return new(data, (int)size);
        }
    }
}
