using System;
using AceLand.EventDriven.Bus.Builders;

namespace AceLand.EventDriven.Bus
{
    public static class EventRaiserExtensions
    {
        public static void Raise(this EventRaiserBuilders.IEventRaiser<IEvent> raiser)
        {
            if (raiser is EventRaiserBuilders.IEventRaiserInternal internalRaiser)
                internalRaiser.InternalRaise(null);
            else
                throw new InvalidOperationException("Invalid raiser implementation.");
        }

        public static EventRaiserBuilders.IEventRaiserCacheBuilder WithCache(
            this EventRaiserBuilders.IEventRaiser<IEvent> raiser)
        {
            if (raiser is EventRaiserBuilders.IEventRaiserInternal internalRaiser)
                return internalRaiser.InternalWithCache();

            throw new InvalidOperationException("Invalid raiser implementation.");
        }

        public static EventRaiserBuilders.IEventRaiserRaiseBuilder WithData<TData>(
            this EventRaiserBuilders.IEventRaiser<IEvent<TData>> raiser, TData data)
        {
            if (raiser is EventRaiserBuilders.IEventRaiserInternal internalRaiser)
                return internalRaiser.InternalRaiseWithData(data);

            throw new InvalidOperationException("Invalid raiser implementation.");
        }
    }
}
