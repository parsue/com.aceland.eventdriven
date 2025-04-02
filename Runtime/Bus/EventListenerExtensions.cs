using System;

namespace AceLand.EventDriven.Bus
{
    // IEventListener can subscribe listener from this extension 
    public static class EventListenerExtensions
    {
        public static EventListenerBuilder.IEventListenerReceiverBuilder Event<T>(this IEventListener listener)
            where T : class
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException($"Event type {typeof(T).Name} must be an interface.");

            return new EventListenerBuilder.EventListenerReceiverBuilder<T>();
        }
    }
}