using AceLand.EventDriven.EventInterface;
using UnityEngine;

namespace AceLand.EventDriven
{
    internal static class EventDrivenBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialization()
        {
            InterfaceMapping.Initialization();
            InterfaceBinding.Initialization();
        }
    }
}