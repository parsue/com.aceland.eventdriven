#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace AceLand.EventDriven.Bus.Diagnostics
{
    /// <summary>
    /// Editor-only data bridge between the runtime EventBus and the Event Monitor window.
    /// The window subscribes to <see cref="EventRaised"/> to receive live raise records and
    /// queries the snapshot helpers on <see cref="EventBus"/> for the current registry/cache
    /// state. Guarded by UNITY_EDITOR so nothing here ships in a player build.
    /// </summary>
    public static class EventMonitorBridge
    {
        /// <summary>Raised (editor only) immediately after an event is dispatched.</summary>
        public static event Action<EventRaiseRecord> EventRaised;

        /// <summary>
        /// Called from the runtime registry when an event is raised. Captures a best-effort
        /// caller stack frame and forwards a record to any listening editor window.
        /// </summary>
        internal static void NotifyRaise(Type eventType, object payload)
        {
            var handler = EventRaised;
            if (handler is null) return;

            ResolveCaller(out var callerType, out var callerMethod, out var callerFile, out var callerLine);

            var record = new EventRaiseRecord(
                EventRecordKind.Raise,
                eventType,
                UnityEditor.EditorApplication.timeSinceStartup,
                DateTime.Now,
                payload is null ? string.Empty : EventPayloadFormatter.Format(payload),
                callerType, callerMethod, callerFile, callerLine);

            handler.Invoke(record);
        }

        /// <summary>
        /// Called from the runtime cache service when a cached event is cleared. Explicit
        /// API clears (<paramref name="cause"/> = <see cref="EventClearCause.ExplicitCaller"/>)
        /// resolve a best-effort caller stack frame; automatic clears (destroyed target / scene
        /// unload) carry a fixed human-readable reason instead.
        /// </summary>
        internal static void NotifyClear(Type eventType, EventClearCause cause)
        {
            var handler = EventRaised;
            if (handler is null) return;

            string callerType = string.Empty, callerMethod = string.Empty, callerFile = string.Empty;
            var callerLine = 0;
            string clearReason;

            switch (cause)
            {
                case EventClearCause.GameObjectDestroyed:
                    clearReason = "gameobject destroyed";
                    break;
                case EventClearCause.SceneLoaded:
                    clearReason = "scene loaded";
                    break;
                default:
                    ResolveCaller(out callerType, out callerMethod, out callerFile, out callerLine);
                    clearReason = string.Empty;
                    break;
            }

            var record = new EventRaiseRecord(
                EventRecordKind.Clear,
                eventType,
                UnityEditor.EditorApplication.timeSinceStartup,
                DateTime.Now,
                string.Empty,
                callerType, callerMethod, callerFile, callerLine,
                clearReason);

            handler.Invoke(record);
        }

        // Walks the stack to find the first frame outside the EventDriven package. That frame
        // is the user call site that raised the event (script path + line when available).
        private static void ResolveCaller(out string type, out string method, out string file, out int line)
        {
            type = string.Empty;
            method = string.Empty;
            file = string.Empty;
            line = 0;

            var trace = new StackTrace(1, true);
            var frames = trace.GetFrames();
            if (frames is null) return;

            foreach (var frame in frames)
            {
                var m = frame.GetMethod();
                var declaring = m?.DeclaringType;
                if (declaring is null) continue;

                var ns = declaring.Namespace;
                if (ns != null && ns.StartsWith("AceLand.EventDriven", StringComparison.Ordinal))
                    continue;

                type = declaring.FullName ?? declaring.Name;
                method = m.Name;
                file = frame.GetFileName() ?? string.Empty;
                line = frame.GetFileLineNumber();
                return;
            }
        }
    }
}
#endif
