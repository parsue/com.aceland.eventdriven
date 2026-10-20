using AceLand.EventDriven.Bus;
using AceLand.EventDriven.EventSignal.Core;
using UnityEngine;

namespace AceLand.EventDriven
{
    internal static class EventBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialization()
        {
#if UNITY_EDITOR
            // No-domain-reload play mode keeps static signal state alive between play
            // sessions (signals are not auto-disposed on stop playing). Clear them on
            // (re)initialization so no stale signal leaks across sessions. Editor-only;
            // player builds get a fresh static state per process launch.
            Signals.ClearAll();
#endif
            EventBus.Initialize();
        }
    }
}
