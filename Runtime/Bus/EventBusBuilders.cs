using System;

namespace AceLand.EventDriven.Bus
{
    public static class EventBusBuilders
    {
        public interface IEventBusBuilder
        {
            EventRaiserBuilders.IEventRaiserPayloadBuilder WithSender(object sender);
            EventListenerBuilders.IEventKickStartBuilder WithListener(Action<object> listener);
            EventListenerBuilders.IEventKickStartBuilder WithListener<TPayload>(Action<object, TPayload> listener);
            void Unlisten(Action<object> listener);
            void Unlisten<TPayload>(Action<object, TPayload> listener);
        }

        internal class EventBusBuilder<T> : IEventBusBuilder
            where T : IEvent
        {
            public EventRaiserBuilders.IEventRaiserPayloadBuilder WithSender(object sender) =>
                new EventRaiserBuilders.EventBusRaiserBuilder<T>(sender);

            public EventListenerBuilders.IEventKickStartBuilder WithListener(Action<object> listener) =>
                new EventListenerBuilders.EventBusListener<T>(listener);

            public EventListenerBuilders.IEventKickStartBuilder WithListener<TPayload>(Action<object, TPayload> listener) =>
                new EventListenerBuilders.EventBusListener<T, TPayload>(listener);

            public void Unlisten(Action<object> listener) =>
                EventBus.Unsubscribe<T>(listener);

            public void Unlisten<TPayload>(Action<object, TPayload> listener) =>
                EventBus.Unsubscribe<T, TPayload>(listener);
        }
    }
}