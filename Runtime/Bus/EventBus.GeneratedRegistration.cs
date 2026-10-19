using System;
using System.Collections.Generic;
using System.ComponentModel;
using AceLand.EventDriven.Bus.Core;

namespace AceLand.EventDriven.Bus
{
    public static partial class EventBus
    {
        // Shared, process-lifetime store of event signatures. Populated by
        // source-generated per-assembly bootstrap via __RegisterGeneratedEvent.
        // Lives here (not in SignatureService) so registrations arriving BEFORE
        // EventBus.Initialize() are retained regardless of init order.
        private static readonly Dictionary<Type, EventSignature> GeneratedSignatures =
            new(32);

        private static readonly object GeneratedSignaturesLock = new();

        /// <summary>
        /// Generated-use only. Invoked by the source-generated per-assembly bootstrap
        /// ([RuntimeInitializeOnLoadMethod]) to push a compile-time event signature.
        /// Do NOT call this from user code.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Generated-use only. Do not call from user code.", error: false)]
        public static void __RegisterGeneratedEvent(
            Type eventType, string displayName, string description,
            bool hasPayload, Type payloadType,
            Func<object, Delegate> binder, Action<Delegate, object> cacheSender)
        {
            if (eventType is null) throw new ArgumentNullException(nameof(eventType));
            if (binder is null) throw new ArgumentNullException(nameof(binder));
            if (cacheSender is null) throw new ArgumentNullException(nameof(cacheSender));

            var kind = hasPayload ? EventSignatureKind.SinglePayload : EventSignatureKind.NoPayload;
            var sig = new EventSignature(eventType, displayName, description, kind, payloadType, binder, cacheSender);

            lock (GeneratedSignaturesLock)
            {
                GeneratedSignatures[eventType] = sig;
            }
        }

        internal static IReadOnlyDictionary<Type, EventSignature> GeneratedSignatureStore
            => GeneratedSignatures;
    }
}
