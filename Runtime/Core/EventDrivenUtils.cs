#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
using UnityEngine;

namespace AceLand.EventDriven.Core
{

#if UNITY_6000_5_OR_NEWER
    internal static partial class EventDrivenUtils
#else
    internal static class EventDrivenUtils
#endif
    {
        internal const float DefaultSignalGetterTimeout = 1.5f;

        /// <summary>
        /// Timeout (seconds) used by signal getters while waiting for a signal to appear.
        /// Runtime holds the value directly (zero dependency). The Editor settings provider
        /// may overwrite this at startup when a configured value exists.
        /// </summary>
        public static float SignalGetterTimeout { get; internal set; } = DefaultSignalGetterTimeout;

#if !UNITY_6000_5_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void ClearStatic()
        {
            SignalGetterTimeout = DefaultSignalGetterTimeout;
        }
#endif
    }
}
