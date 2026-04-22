using System;

namespace AceLand.EventDriven.Bus.Builders
{
    public static class EventRaiserBuilders
    {
        public interface IEventRaiser<out TEvent> where TEvent : IBusEvent
        {
            Type EventType { get; }
        }

        public interface IEventRaiserRaiseBuilder
        {
            void Raise();
        }

        internal interface IEventRaiserInternal
        {
            void InternalRaise();
            IEventRaiserRaiseBuilder InternalWithData<TPayload>(TPayload data);
        }

        internal class EventBusRaiser<TEvent> : IEventRaiser<TEvent>, IEventRaiserInternal
            where TEvent : IBusEvent
        {
            public Type EventType => typeof(TEvent);

            public void InternalRaise() =>
                EventBus.RaiseEvent(typeof(TEvent));

            public IEventRaiserRaiseBuilder InternalWithData<TPayload>(TPayload data) =>
                new EventBusRaiserWithData<TPayload>(typeof(TEvent), data);
        }

        internal class EventBusRaiserWithData<TPayload> : IEventRaiserRaiseBuilder
        {
            private readonly Type _eventType;
            private readonly TPayload _payload;

            public EventBusRaiserWithData(Type eventType, TPayload payload)
            {
                _eventType = eventType;
                _payload = payload;
            }

            public void Raise() =>
                EventBus.RaiseEvent(_eventType, _payload);
        }
    }
}