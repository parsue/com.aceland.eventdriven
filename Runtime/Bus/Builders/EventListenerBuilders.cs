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
            where TEvent : IEvent
        {
            private readonly Action<object> _listener;

            public EventBusListener(Action<object> listener) =>
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
            where TEvent : IEvent
        {
            private readonly Action<object, TPayload> _listener;

            public EventBusListener(Action<object, TPayload> listener) =>
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