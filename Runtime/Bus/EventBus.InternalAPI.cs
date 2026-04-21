using System;

namespace AceLand.EventDriven.Bus
{
    public static partial class EventBus
    {
        internal static void EnsureIsEventInterface(Type t)
        {
            if (t is not { IsInterface: true } || !typeof(IBusEvent).IsAssignableFrom(t))
                throw new ArgumentException($"Event type {t?.Name} must be an interface implementing IBusEvent.");
        }

        internal static void SubscribeInstance(Type eventType, object instance)
        {
            Registry.SubscribeInstance(eventType, instance);
        }

        internal static void UnsubscribeInstance(Type eventType, object instance)
        {
            Registry.UnsubscribeInstance(eventType, instance);
        }

        internal static void UnsubscribeAllForInstance(object instance)
        {
            Registry.UnsubscribeAllForInstance(instance);
        }

        internal static void SubscribeDelegate<TEvent>(Action<object> listener)
            where TEvent : IBusEvent
        {
            Registry.SubscribeDelegate(typeof(TEvent), listener);
        }

        internal static void SubscribeDelegate<TEvent, TPayload>(Action<object, TPayload> listener)
            where TEvent : IBusEvent
        {
            Registry.SubscribeDelegate(typeof(TEvent), listener);
        }

        internal static void KickStartInstance(Type eventType, object instance)
        {
            Registry.KickStartInstance(eventType, instance);
        }

        internal static void SendEventCache<TEvent>(Action<object> listener) where TEvent : IBusEvent
        {
            Registry.SendCacheToDelegate(typeof(TEvent), listener);
        }

        internal static void SendEventCache<TEvent, TPayload>(Action<object, TPayload> listener)
            where TEvent : IBusEvent
        {
            Registry.SendCacheToDelegate(typeof(TEvent), listener);
        }

        internal static void RaiseEvent(Type eventType, object sender)
        {
            Registry.RaiseEvent(eventType, sender);
        }

        internal static void RaiseEvent<TPayload>(Type eventType, object sender, TPayload payload)
        {
            Registry.RaiseEvent(eventType, sender, payload);
        }
    }
}