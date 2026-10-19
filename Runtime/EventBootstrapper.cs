using AceLand.EventDriven.Bus;
using UnityEngine;

namespace AceLand.EventDriven
{
    internal static class EventBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialization()
        {
            EventBus.Initialize();
        }
    }
}