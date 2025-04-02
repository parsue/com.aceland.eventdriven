using System;

namespace AceLand.EventDriven.Bus
{
    // IEventRaiser can raise event from this extension 
    public static class EventRaiserExtensions
    {
        public static EventBusRaiserBuilder.IEventRaiserDataBuilder Event<T>(this IEventRaiser raiser)
            where T : class
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException($"Event type {typeof(T).Name} must be an interface.");

            return new EventBusRaiserBuilder.EventRaiserBuilder<T>(raiser);
        }
    }
}