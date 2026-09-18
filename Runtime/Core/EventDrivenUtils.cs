using AceLand.EventDriven.ProjectSetting;
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
        public static EventDrivenSettings Settings
        {
            get
            {
                _settings ??= Resources.Load<EventDrivenSettings>(nameof(EventDrivenSettings));
                return _settings;
            }
        }
        
#if UNITY_6000_5_OR_NEWER
        [AutoStaticsCleanup]
#endif
        private static EventDrivenSettings _settings;

#if !UNITY_6000_5_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void ClearStatic()
        {
            _settings = null;
        }
#endif
    }
}