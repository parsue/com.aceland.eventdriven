using System;

namespace AceLand.EventDriven.Bus.Builders
{
    public static class EventListenerBuilders
    {
        public interface IEventKickStartBuilder : IEventListenerBuilder
        {
            void KickStart();
        }

        public interface IEventListenerBuilder
        {
            IEventKickStartBuilder Listen();
        }

        internal class EventBusListener<TEvent> : IEventKickStartBuilder
            where TEvent : IBusEvent
        {
            private readonly Action _listener;

            public EventBusListener(Action listener) =>
                _listener = listener ?? throw new ArgumentNullException(nameof(listener));

            public IEventKickStartBuilder Listen()
            {
                EventBus.SubscribeDelegate<TEvent>(_listener);
                return this;
            }

            public void KickStart()
            {
                EventBus.SendEventCache<TEvent>(_listener);
            }
        }

        internal class EventBusListener<TEvent, TPayload> : IEventKickStartBuilder
            where TEvent : IBusEvent
        {
            private readonly Action<TPayload> _listener;

            public EventBusListener(Action<TPayload> listener) =>
                _listener = listener ?? throw new ArgumentNullException(nameof(listener));

            public IEventKickStartBuilder Listen()
            {
                EventBus.SubscribeDelegate<TEvent, TPayload>(_listener);
                return this;
            }

            public void KickStart()
            {
                EventBus.SendEventCache<TEvent, TPayload>(_listener);
            }
        }
    }
}