using System;

namespace AceLand.EventDriven.Bus.Builders
{
    public static class EventRaiserBuilders
    {
        // Notice the covariant 'out' keyword here
        public interface IEventRaiser<out TEvent> where TEvent : IBusEvent
        {
            Type EventType { get; }
            object Sender { get; }
        }

        public interface IEventRaiserRaiseBuilder
        {
            void Raise();
        }

        // Internal interface to hide the actual raise/withData logic from the public API
        internal interface IEventRaiserInternal
        {
            void InternalRaise();
            IEventRaiserRaiseBuilder InternalWithData<TPayload>(TPayload data);
        }

        internal class EventBusRaiser<TEvent> : IEventRaiser<TEvent>, IEventRaiserInternal
            where TEvent : IBusEvent
        {
            public Type EventType => typeof(TEvent);
            public object Sender { get; }

            public EventBusRaiser(object sender)
            {
                Sender = sender;
            }

            public void InternalRaise() =>
                EventBus.RaiseEvent(typeof(TEvent), Sender);

            public IEventRaiserRaiseBuilder InternalWithData<TPayload>(TPayload data) =>
                new EventBusRaiserWithData<TPayload>(typeof(TEvent), Sender, data);
        }

        internal class EventBusRaiserWithData<TPayload> : IEventRaiserRaiseBuilder
        {
            private readonly Type _eventType;
            private readonly object _sender;
            private readonly TPayload _payload;

            public EventBusRaiserWithData(Type eventType, object sender, TPayload payload)
            {
                _eventType = eventType;
                _sender = sender;
                _payload = payload;
            }

            public void Raise() =>
                EventBus.RaiseEvent(_eventType, _sender, _payload);
        }
    }
}