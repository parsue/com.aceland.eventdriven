#if UNITY_EDITOR
using System;
using System.Reflection;
using AceLand.EventDriven.Bus.Core;

namespace AceLand.EventDriven.Bus.Diagnostics
{
    /// <summary>
    /// Editor-only snapshot of a single registered event interface, surfaced to the
    /// Event Monitor window. Compiled only under UNITY_EDITOR so it never ships in a
    /// player build.
    /// </summary>
    public readonly struct EventMonitorEntry
    {
        public readonly Type InterfaceType;
        public readonly string DisplayName;
        public readonly string Description;
        public readonly bool HasPayload;
        public readonly Type PayloadType;
        public readonly int ListenerCount;
        public readonly bool IsCached;

        public EventMonitorEntry(Type interfaceType, string displayName, string description,
            bool hasPayload, Type payloadType, int listenerCount, bool isCached)
        {
            InterfaceType = interfaceType;
            DisplayName = displayName;
            Description = description ?? string.Empty;
            HasPayload = hasPayload;
            PayloadType = payloadType;
            ListenerCount = listenerCount;
            IsCached = isCached;
        }
    }

    /// <summary>
    /// Editor-only description of a single live listener bound to an event (play mode).
    /// Used by the monitor inspector for listener listing + source tracing.
    /// </summary>
    public readonly struct EventListenerInfo
    {
        public readonly Type TargetType;
        public readonly string MethodName;

        /// <summary>Non-null when the listener target is a UnityEngine.Object (pingable).</summary>
        public readonly UnityEngine.Object UnityTarget;

        /// <summary>True for instance listeners; false for static delegates.</summary>
        public readonly bool IsInstance;

        public EventListenerInfo(MethodInfo method, object target)
        {
            MethodName = method?.Name ?? "<unknown>";
            IsInstance = target != null;
            TargetType = target?.GetType() ?? method?.DeclaringType;
            UnityTarget = target as UnityEngine.Object;
        }
    }

    /// <summary>
    /// Editor-only snapshot of an event's current cache state (play mode).
    /// </summary>
    public readonly struct EventCacheInfo
    {
        public readonly bool HasCache;
        public readonly CacheBinding Binding;
        public readonly string PayloadString;

        public EventCacheInfo(bool hasCache, CacheBinding binding, string payloadString)
        {
            HasCache = hasCache;
            Binding = binding;
            PayloadString = payloadString ?? "null";
        }
    }

    /// <summary>Editor-only kind for a history record: an event raise or a cache clear.</summary>
    public enum EventRecordKind
    {
        Raise,
        Clear,
    }

    /// <summary>
    /// Editor-only cause classification for a cache clear, used to build the history
    /// record's displayed reason.
    /// </summary>
    public enum EventClearCause
    {
        /// <summary>User API clear (ClearCache); caller is resolved from the stack.</summary>
        ExplicitCaller,
        /// <summary>Bound target (GameObject/Object) was destroyed, invalidating the cache.</summary>
        GameObjectDestroyed,
        /// <summary>Scene-bound cache dropped because its scene unloaded.</summary>
        SceneLoaded,
    }

    /// <summary>
    /// Editor-only record of a single raise or cache-clear, captured for the live monitor
    /// update highlight and the per-event history window. Caller info is best-effort
    /// (resolved from a play-mode stack trace). Clear records carry a <see cref="ClearReason"/>
    /// when the clear was not triggered by a traceable user call site.
    /// </summary>
    public readonly struct EventRaiseRecord
    {
        public readonly EventRecordKind Kind;
        public readonly Type InterfaceType;
        public readonly string InterfaceName;
        public readonly double EditorTime;
        public readonly DateTime WallClock;
        public readonly string PayloadString;

        public readonly string CallerType;
        public readonly string CallerMethod;
        public readonly string CallerFile;
        public readonly int CallerLine;

        /// <summary>Non-empty for clear records without a traceable caller (e.g. "gameobject destroyed").</summary>
        public readonly string ClearReason;

        public EventRaiseRecord(EventRecordKind kind, Type interfaceType, double editorTime, DateTime wallClock,
            string payloadString, string callerType, string callerMethod, string callerFile, int callerLine,
            string clearReason = null)
        {
            Kind = kind;
            InterfaceType = interfaceType;
            InterfaceName = interfaceType?.Name ?? "<unknown>";
            EditorTime = editorTime;
            WallClock = wallClock;
            PayloadString = payloadString ?? string.Empty;
            CallerType = callerType ?? string.Empty;
            CallerMethod = callerMethod ?? string.Empty;
            CallerFile = callerFile ?? string.Empty;
            CallerLine = callerLine;
            ClearReason = clearReason ?? string.Empty;
        }

        public bool HasCallerSource => !string.IsNullOrEmpty(CallerFile);
    }
}
#endif
