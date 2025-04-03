using System;

namespace AceLand.EventDriven.Bus
{
    public static class EventListenerBuilders
    {
        public interface IEventBusListenerBuilder
        {
            IEventKickStartBuilder WithListener(Action<object> listener);
            IEventKickStartBuilder WithListener<TPayload>(Action<object, TPayload> listener);
            void Unlisten(Action<object> listener);
            void Unlisten<TPayload>(Action<object, TPayload> listener);
        }

        public interface IEventKickStartBuilder : IEventListenerBuilder
        {
            IEventListenerBuilder WithKickStart();
        }
        
        public interface IEventListenerBuilder
        {
            void Listen();
        }
        
        internal class EventBusEventListenerBuilder<T> : IEventBusListenerBuilder
            where T : class
        {
            public IEventKickStartBuilder WithListener(Action<object> listener) =>
                new EventBusListener<T>(listener);

            public IEventKickStartBuilder WithListener<TPayload>(Action<object, TPayload> listener) =>
                new EventBusListener<T, TPayload>(listener);

            public void Unlisten(Action<object> listener) =>
                EventBus.Unsubscribe<T>(listener);

            public void Unlisten<TPayload>(Action<object, TPayload> listener) =>
                EventBus.Unsubscribe<T, TPayload>(listener);
        }

        private class EventBusListener<T> : IEventKickStartBuilder
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

        private class EventBusListener<T, TPayload> : IEventKickStartBuilder
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