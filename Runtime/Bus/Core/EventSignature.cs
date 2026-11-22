using System;

namespace AceLand.EventDriven.Bus.Core
{
    /// <summary>
    /// Compile-time produced metadata + glue for a single event interface.
    /// Populated by source-generated bootstrap code (zero reflection at runtime).
    /// </summary>
    internal sealed class EventSignature
    {
        public Type EventInterfaceType { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public EventSignatureKind Kind { get; }
        public Type PayloadTypeOrNull { get; }

        /// <summary>instance =&gt; strongly-typed Action / Action&lt;T&gt; (no reflection).</summary>
        public Func<object, Delegate> Binder { get; }

        /// <summary>
        /// (combinedDelegate, typedCachePayload) =&gt; typed invoke (no DynamicInvoke, no unbox).
        /// The payload arrives as the strongly-typed <see cref="CachePayload{T}"/> boxed behind
        /// <see cref="ICachePayload"/>; the generated sender reads its typed <c>Value</c> field.
        /// </summary>
        public Action<Delegate, ICachePayload> CacheSender { get; }

        public EventSignature(Type eventInterfaceType, string displayName, string description,
            EventSignatureKind kind, Type payloadTypeOrNull,
            Func<object, Delegate> binder, Action<Delegate, ICachePayload> cacheSender)
        {
            EventInterfaceType = eventInterfaceType;
            DisplayName = displayName;
            Description = description;
            Kind = kind;
            PayloadTypeOrNull = payloadTypeOrNull;
            Binder = binder;
            CacheSender = cacheSender;
        }
    }
}
