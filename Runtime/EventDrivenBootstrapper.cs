using AceLand.EventDriven.EventInterface;
using AceLand.EventDriven.ProjectSetting;
using UnityEngine;

namespace AceLand.EventDriven
{
    internal static class EventDrivenBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialization()
        {
            EventDrivenHelper.Settings = Resources.Load<EventDrivenSettings>(nameof(EventDrivenSettings));
            
            InterfaceMapping.InitInterfaceToComponentMapping();
        }
    }
}