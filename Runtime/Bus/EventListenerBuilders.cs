using System;

namespace AceLand.EventDriven.Bus
{
    public static class EventListenerBuilders
    {
        public interface IEventKickStartBuilder : IEventListenerBuilder
        {
            IEventListenerBuilder WithKickStart();
        }
        
        public interface IEventListenerBuilder
        {
            void Listen();
        }

        internal class EventBusListener<T> : IEventKickStartBuilder
            where T : class
        {
            private readonly Action<object> _listener;
            private bool _kickStart;
            
            public  EventBusListener(Action<object> listener) =>
                _listener = listener;

            public IEventListenerBuilder WithKickStart()
            {
                _kickStart = true;
                return this;
            }

            public void Listen()
            {
                EventBus.Subscribe<T>(_listener);
                if (!_kickStart) return;
                EventBus.SendEventCache<T>(_listener);
            }
        }

        internal class EventBusListener<T, TPayload> : IEventKickStartBuilder
            where T : class
        {
            private readonly Action<object, TPayload> _listener;
            private bool _kickStart;
            
            public  EventBusListener(Action<object, TPayload> listener) =>
                _listener = listener;

            public IEventListenerBuilder WithKickStart()
            {
                _kickStart = true;
                return this;
            }

            public void Listen()
            {
                EventBus.Subscribe<T, TPayload>(_listener);
                if (!_kickStart) return;
                EventBus.SendEventCache<T, TPayload>(_listener);
            }
        }
    }
}