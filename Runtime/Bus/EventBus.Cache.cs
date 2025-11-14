using System;
using System.Collections.Generic;

namespace AceLand.EventDriven.Bus
{
    public static partial class EventBus
    {
        private static readonly Dictionary<Type, EventCache> eventCache = new();
        
        private static void ClearEventCache<TEvent>() where TEvent : IEvent
        {
            lock (@lock) eventCache.Remove(typeof(TEvent));
        }

        private static void ClearAllEventCache()
        {
            lock (@lock) eventCache.Clear();
        }
    }
}