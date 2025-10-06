using AceLand.EventDriven.Core;
using UnityEngine;

namespace AceLand.EventDriven
{
    internal static class EventBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialization()
        {
            var settings = EventDrivenUtils.Settings;
            foreach (var provider in settings.PrewarmProviders)
                provider.PrewarmSignal();
        }
    }
}