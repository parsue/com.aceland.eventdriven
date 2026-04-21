using System;
using AceLand.EventDriven.Bus.Builders;

namespace AceLand.EventDriven.Bus
{
    public static class EventRaiserExtensions
    {
        // Only available for events implementing IEventNoData
        public static void Raise(this EventRaiserBuilders.IEventRaiser<IEvent> raiser)
        {
            if (raiser is EventRaiserBuilders.IEventRaiserInternal internalRaiser)
                internalRaiser.InternalRaise();
            else
                throw new InvalidOperationException("Invalid raiser implementation.");
        }

        // Only available for events implementing IEventWithData<TData>
        // The compiler will automatically infer TData for you!
        public static EventRaiserBuilders.IEventRaiserRaiseBuilder WithData<TData>(
            this EventRaiserBuilders.IEventRaiser<IEvent<TData>> raiser, TData data)
        {
            if (raiser is EventRaiserBuilders.IEventRaiserInternal internalRaiser)
                return internalRaiser.InternalWithData(data);

            throw new InvalidOperationException("Invalid raiser implementation.");
        }
    }
}