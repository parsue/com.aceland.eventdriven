using System;
using System.Collections.Generic;
using AceLand.EventDriven.Bus.Diagnostics;

namespace AceLand.EventDriven.Editor.Monitor
{
    /// <summary>
    /// Mutable per-event row the Event Monitor window binds to. One instance lives
    /// for the whole lifetime of a registered event interface (across refreshes) so
    /// transient UI state (highlight timer, last raise) survives list rebuilds.
    /// </summary>
    internal sealed class EventMonitorRow
    {
        public Type InterfaceType { get; }
        public string InterfaceName { get; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool HasPayload { get; set; }
        public Type PayloadType { get; set; }

        // Live play-mode state.
        public int ListenerCount { get; set; }
        public bool IsCached { get; set; }
        public int RaiseCount { get; set; }

        /// <summary>EditorApplication.timeSinceStartup of the last raise (0 = never).</summary>
        public double LastRaiseEditorTime { get; set; }
        public DateTime LastRaiseWallClock { get; set; }

        /// <summary>Remaining highlight seconds (counts down from 5 → 0).</summary>
        public double HighlightRemaining { get; set; }

        /// <summary>Bounded raise history (newest last) for the per-event history window.</summary>
        public readonly List<EventRaiseRecord> History = new();

        public EventMonitorRow(Type interfaceType, string interfaceName)
        {
            InterfaceType = interfaceType;
            InterfaceName = interfaceName ?? interfaceType?.Name ?? "<unknown>";
        }

        public bool HasEverRaised => LastRaiseEditorTime > 0d;

        public string StatusLabel
        {
            get
            {
                if (ListenerCount <= 0) return "No Listener";
                if (!HasEverRaised) return "Idle";
                return "Active";
            }
        }
    }
}
