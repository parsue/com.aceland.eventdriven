using System;
using System.Collections.Generic;

namespace AceLand.EventDriven.Bus
{
    public static class EventBus
    {
        // event without payload <IEvent, Listener<sender>>
        private static readonly Dictionary<Type, Action<object>> events = new();
        // event with payload <IEvent, Listener<sender, IEventData>
        private static readonly Dictionary<Type, Delegate> eventsWithPayload = new();
        // latest IEventData <IEvent, EventCache>
        private static readonly Dictionary<Type, EventCache> eventCache = new();

        public static void ClearCache<T>() where T : IEvent =>
            events.Remove(typeof(T));
        
        public static void ClearAllCache() =>
            events.Clear();
        
        // user entrance to raise event or subscribe listener
        public static EventBusBuilders.IEventBusBuilder Event<T>()
            where T : IEvent
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException($"Event type {typeof(T).Name} must be an interface.");

            return new EventBusBuilders.EventBusBuilder<T>();
        }

        // Subscribe a new Listener without payload for listener
        internal static void Subscribe<T>(Action<object> listener)
            where T : IEvent
        {
            if (events.ContainsKey(typeof(T)))
                events[typeof(T)] += listener;
            else
                events[typeof(T)] = listener;
        }
        
        // Subscribe a new Listener with payload for listener
        internal static void Subscribe<T, TPayload>(Action<object, TPayload> listener)
            where T : IEvent
        {
            if (eventsWithPayload.ContainsKey(typeof(T)))
                eventsWithPayload[typeof(T)] = Delegate.Combine(eventsWithPayload[typeof(T)], listener);
            else
                eventsWithPayload[typeof(T)] = listener;
        }

        // Unsubscribe a new Listener without payload
        internal static void Unsubscribe<T>(Action<object> listener)
            where T : IEvent
        {
            if (!events.ContainsKey(typeof(T))) return;
            events[typeof(T)] -= listener;
        }

        // Unsubscribe a new Listener with payload
        internal static void Unsubscribe<T, TPayload>(Action<object, TPayload> listener)
            where T : IEvent
        {
            if (!eventsWithPayload.ContainsKey(typeof(T))) return;
            eventsWithPayload[typeof(T)] = Delegate.Remove(eventsWithPayload[typeof(T)], listener);
        }
        
        // Send Event Cache without payload, StartKick will send after Subscribed
        internal static void SendEventCache<T>(Action<object> listener)
            where T : IEvent
        {
            if (!eventCache.ContainsKey(typeof(T))) return;
            var cache = eventCache[typeof(T)];
            listener.Invoke(cache.Sender);
        }

        // Send Event Cache with payload, StartKick will send after Subscribed
        internal static void SendEventCache<T, TPayload>(Action<object, TPayload> listener)
            where T : IEvent
        {
            if (!eventCache.ContainsKey(typeof(T))) return;
            var cache = eventCache[typeof(T)];
            listener.Invoke(cache.Sender, (TPayload)cache.EventData);
        }
        
        // Raise an Event without payload to listeners
        internal static void RaiseEvent<T>(object sender)
            where T : IEvent
        {
            eventCache[typeof(T)] = new EventCache(sender, null);
            if (events.ContainsKey(typeof(T)))
                events[typeof(T)]?.Invoke(sender);
        }
        
        // Raise an Event with payload to listeners
        internal static void RaiseEvent<T, TPayload>(object sender, TPayload payload)
            where T : IEvent
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException($"Event type {typeof(T).Name} must be an interface.");

            eventCache[typeof(T)] = new EventCache(sender, payload);
            if (eventsWithPayload.ContainsKey(typeof(T)))
                (eventsWithPayload[typeof(T)] as Action<object, TPayload>)?.Invoke(sender, payload);
        }
    }
}