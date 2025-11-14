using System;

namespace AceLand.EventDriven.Bus
{
    public static partial class EventBus
    {
        public static EventBusBuilders.IEventBusBuilder Event<TEvent>() where TEvent : IEvent
        {
            EnsureIsEventInterface(typeof(TEvent));
            return new EventBusBuilders.EventBusBuilder<TEvent>(null);
        }

        public static EventBusBuilders.IEventBusObjBuilder Event(object listenerInstance)
        {
            return listenerInstance == null
                ? throw new ArgumentNullException(nameof(listenerInstance))
                : new EventBusBuilders.MultiEventBusBuilder(listenerInstance);
        }

        public static EventBusBuilders.IEventBusObjBuilder Event<TEvent>(object listenerInstance) where TEvent : IEvent
        {
            EnsureIsEventInterface(typeof(TEvent));
            return listenerInstance == null
                ? throw new ArgumentNullException(nameof(listenerInstance))
                : new EventBusBuilders.EventBusBuilder<TEvent>(listenerInstance);
        }
        
        public static void ClearCache<TEvent>() where TEvent : IEvent
        {
            ClearEventCache<TEvent>();
        }

        public static void ClearAllCache()
        {
            ClearAllEventCache();
        }
    }
}