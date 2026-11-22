using AceLand.EventDriven.Bus.Builders;

namespace AceLand.EventDriven.Bus
{
    public static class EventRaiserExtensions
    {
        public static void Raise(this EventRaiserBuilders.IEventRaiser<IEvent> raiser)
        {
            EventBus.RaiseEvent(raiser.EventType, null);
        }

        public static EventRaiserBuilders.EventBusCacheBuilder WithCache(
            this EventRaiserBuilders.IEventRaiser<IEvent> raiser)
        {
            return new EventRaiserBuilders.EventBusCacheBuilder(raiser.EventType, Core.CacheSpec.Persistent);
        }

        public static EventRaiserBuilders.EventBusRaiserWithData<TData> WithData<TData>(
            this EventRaiserBuilders.IEventRaiser<IEvent<TData>> raiser, TData data)
        {
            return new EventRaiserBuilders.EventBusRaiserWithData<TData>(raiser.EventType, data);
        }
    }
}
