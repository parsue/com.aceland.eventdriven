#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using AceLand.EventDriven.Bus.Diagnostics;

namespace AceLand.EventDriven.Bus
{
    /// <summary>
    /// Editor-only public query surface consumed by the Event Monitor window.
    /// Compiled only under UNITY_EDITOR so it never ships in a player build.
    ///
    /// The window lives in a separate public asmdef (AceLand.EventDriven.Editor)
    /// and there is no InternalsVisibleTo, so this partial exposes a public,
    /// read-only bridge over the internal services (Signatures / Registry / Cache).
    /// </summary>
    public static partial class EventBus
    {
        /// <summary>
        /// Snapshot every registered event interface. Reads the authoritative
        /// source-generated signature store (populated by every assembly's
        /// per-assembly bootstrap) so it works in both edit mode (InitializeOnLoad
        /// push) and play mode, and is immune to init-order gaps that would make a
        /// one-time <see cref="Services.SignatureService"/> snapshot miss late /
        /// cross-assembly registrations.
        /// </summary>
        public static IReadOnlyList<EventMonitorEntry> EditorGetMonitorEntries()
        {
            var result = new List<EventMonitorEntry>();

            foreach (var sig in EditorSnapshotSignatures())
            {
                var type = sig.EventInterfaceType;
                if (type is null) continue;

                var hasPayload = sig.PayloadTypeOrNull != null;
                var listenerCount = Registry?.EditorGetListenerCount(type) ?? 0;
                var isCached = Cache != null
                    && Cache.EditorGetCacheInfo(type).HasCache;

                result.Add(new EventMonitorEntry(
                    type,
                    sig.DisplayName,
                    sig.Description,
                    hasPayload,
                    sig.PayloadTypeOrNull,
                    listenerCount,
                    isCached));
            }

            return result;
        }

        /// <summary>
        /// Try to snapshot a single event interface by type. Reads the authoritative
        /// source-generated signature store (see <see cref="EditorGetMonitorEntries"/>).
        /// </summary>
        public static bool EditorTryGetMonitorEntry(Type eventType, out EventMonitorEntry entry)
        {
            entry = default;
            if (eventType is null) return false;

            Core.EventSignature sig;
            lock (GeneratedSignaturesLock)
            {
                if (!GeneratedSignatures.TryGetValue(eventType, out sig)) return false;
            }

            var hasPayload = sig.PayloadTypeOrNull != null;
            var listenerCount = Registry?.EditorGetListenerCount(eventType) ?? 0;
            var isCached = Cache != null && Cache.EditorGetCacheInfo(eventType).HasCache;

            entry = new EventMonitorEntry(
                eventType,
                sig.DisplayName,
                sig.Description,
                hasPayload,
                sig.PayloadTypeOrNull,
                listenerCount,
                isCached);
            return true;
        }

        // Thread-safe snapshot of the generated signature store (populated before
        // and after EventBus.Initialize, across every referencing assembly).
        private static List<Core.EventSignature> EditorSnapshotSignatures()
        {
            lock (GeneratedSignaturesLock)
                return new List<Core.EventSignature>(GeneratedSignatures.Values);
        }

        /// <summary>
        /// Live listeners bound to an event (play mode). Empty in edit mode or
        /// before initialization.
        /// </summary>
        public static IReadOnlyList<EventListenerInfo> EditorGetListeners(Type eventType)
        {
            if (eventType is null || Registry is null)
                return Array.Empty<EventListenerInfo>();
            return Registry.EditorGetListeners(eventType);
        }

        /// <summary>
        /// Current cache state of an event (play mode).
        /// </summary>
        public static EventCacheInfo EditorGetCacheInfo(Type eventType)
        {
            if (eventType is null || Cache is null)
                return new EventCacheInfo(false, Core.CacheBinding.Persistent, "null");
            return Cache.EditorGetCacheInfo(eventType);
        }
    }
}
#endif
