#if ENABLE_PROFILER
using System;
using System.Collections.Generic;
using Unity.Profiling;

namespace AceLand.EventDriven
{
    // Editor/Profiler-only helper that produces cached ProfilerMarkers for
    // EventBus and Signal dispatch. All markers are created once and reused to
    // keep per-raise allocations at zero (apart from Delegate.GetInvocationList,
    // which only exists in profiler builds). Names never contain '/'.
    internal static class EventProfiler
    {
        private const string BusPrefix = "AceLand.EventDriven.Bus.";

        // Bus top-level scope keyed by event type (no per-raise string build).
        private static readonly Dictionary<Type, BusScopeEntry> BusScopes = new();

        // Signal/Linker top-level scope keyed by its already-built name string.
        private static readonly Dictionary<string, ProfilerMarker> NamedScopes = new();

        // Per-listener markers keyed by the actual delegate instance so the
        // source-function name is reflected only once per subscription.
        private static readonly Dictionary<Delegate, ProfilerMarker> ListenerMarkers = new();

        private readonly struct BusScopeEntry
        {
            public BusScopeEntry(ProfilerMarker marker, string prefix)
            {
                Marker = marker;
                Prefix = prefix;
            }

            public readonly ProfilerMarker Marker;
            public readonly string Prefix;
        }

        internal static ProfilerMarker BusScope(Type eventType, out string prefix)
        {
            if (BusScopes.TryGetValue(eventType, out var entry))
            {
                prefix = entry.Prefix;
                return entry.Marker;
            }

            prefix = BusPrefix + eventType.Name;
            var marker = new ProfilerMarker(prefix);
            BusScopes[eventType] = new BusScopeEntry(marker, prefix);
            return marker;
        }

        internal static ProfilerMarker NamedScope(string name)
        {
            if (NamedScopes.TryGetValue(name, out var marker))
                return marker;

            marker = new ProfilerMarker(name);
            NamedScopes[name] = marker;
            return marker;
        }

        internal static ProfilerMarker Listener(string prefix, Delegate listener)
        {
            if (ListenerMarkers.TryGetValue(listener, out var marker))
                return marker;

            var method = listener.Method;
            var declaringType = method.DeclaringType?.Name;
            var name = string.IsNullOrEmpty(declaringType)
                ? prefix + "." + method.Name
                : prefix + "." + declaringType + "." + method.Name;

            marker = new ProfilerMarker(name);
            ListenerMarkers[listener] = marker;
            return marker;
        }
    }
}
#endif
