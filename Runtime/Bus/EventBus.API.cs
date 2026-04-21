using System;
using AceLand.EventDriven.Bus.Builders;

namespace AceLand.EventDriven.Bus
{
    public static partial class EventBus
    {
        public static EventBusBuilders.IEventBusBuilder<TEvent> Event<TEvent>() where TEvent : IBusEvent
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

        public static EventBusBuilders.IEventBusObjBuilder Event<TEvent>(object listenerInstance) where TEvent : IBusEvent
        {
            EnsureIsEventInterface(typeof(TEvent));
            return listenerInstance == null
                ? throw new ArgumentNullException(nameof(listenerInstance))
                : new EventBusBuilders.EventBusBuilder<TEvent>(listenerInstance);
        }

        public static void ClearCache<TEvent>() where TEvent : IBusEvent
        {
            Cache.ClearEventCache(typeof(TEvent));
        }

        public static void ClearAllCache()
        {
            Cache.ClearAllEventCache();
        }
    }
}