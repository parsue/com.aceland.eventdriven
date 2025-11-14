using System;
using System.Linq;
using System.Reflection;
using AceLand.EventDriven.Bus;
using AceLand.EventDriven.Core;
using UnityEngine;

namespace AceLand.EventDriven
{
    internal static class EventBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialization()
        {
            InitEventBus();
            var settings = EventDrivenUtils.Settings;
            foreach (var provider in settings.PrewarmProviders)
                provider.PrewarmSignal();
        }

        private static void InitEventBus()
        {
            // Discover all interfaces that implement IEvent
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var iEvent = typeof(IEvent);

            var eventInterfaces = assemblies
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
                })
                .Where(t => t != null && t.IsInterface && iEvent.IsAssignableFrom(t) && t != iEvent)
                .ToArray();

            // Register event signatures into the bus
            EventBus.BootstrapRegister(eventInterfaces);

            // Optionally: log discovered listeners (classes implementing these interfaces)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var listenerTypes = assemblies
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
                })
                .Where(t => t != null && t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => eventInterfaces.Contains(i)))
                .ToArray();

            Debug.Log($"[EventBootstrapper] Registered {eventInterfaces.Length} event interfaces. Potential listener types: {listenerTypes.Length}.");
#endif
        }
    }
}