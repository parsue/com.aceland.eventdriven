using System;
using System.ComponentModel;

namespace AceLand.EventDriven.Bus.Core
{
    /// <summary>
    /// Non-generic handle to a cached event payload. Enables heterogeneous storage of
    /// typed payloads inside a single cache dictionary WITHOUT boxing value types:
    /// the concrete <see cref="CachePayload{T}"/> keeps the value in a strongly-typed
    /// field, so writes are a field store (not a box) and typed reads need no unbox.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ICachePayload
    {
        /// <summary>Editor-only: boxes the value for display/formatting. Do not use on hot paths.</summary>
        object BoxedValue { get; }
    }

    /// <summary>
    /// Generated-use-only typed payload box. Referenced by source-generated cache senders,
    /// which read <see cref="Value"/> with the compile-time-known <typeparamref name="T"/>
    /// so no unboxing occurs on the dispatch path. The value field is mutable to allow the
    /// cache service to reuse an existing instance (zero-alloc) on repeated cache raises of
    /// the same event payload type.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CachePayload<T> : ICachePayload
    {
        public T Value;

        public CachePayload(T value) => Value = value;

        public object BoxedValue => Value;
    }
}
