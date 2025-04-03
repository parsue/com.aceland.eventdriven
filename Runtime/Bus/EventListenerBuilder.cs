using System;

namespace AceLand.EventDriven.Bus
{
    // Builder for subscribe an listener
    public static class EventListenerBuilder
    {
        public interface IEventListenerReceiverBuilder
        {
            IEventKickStartBuilder WithListener(Action<object> listener);
            IEventKickStartBuilder WithListener<TPayload>(Action<object, TPayload> listener);
            
            void Unsubscribe(Action<object> listener);
            void Unsubscribe<TPayload>(Action<object, TPayload> listener);
        }
        
        public interface IEventKickStartBuilder : IEventSubscribeBuilder
        {
            IEventSubscribeBuilder WithKickStart();
        }
        
        public interface IEventSubscribeBuilder
        {
            void Subscribe();
        }

        internal class EventListenerReceiverBuilder<T> : IEventListenerReceiverBuilder
            where T : class
        {
            internal EventListenerReceiverBuilder()
            {
                if (!typeof(T).IsInterface)
                    throw new ArgumentException($"Event type {typeof(T).Name} must be an interface.");
            }

            public IEventKickStartBuilder WithListener(Action<object> listener) =>
                new EventListenerSubscriptionBuilder<T>(listener);

            public IEventKickStartBuilder WithListener<TPayload>(Action<object, TPayload> listener) =>
                new EventListenerSubscriptionBuilder<T,TPayload>(listener);

            public void Unsubscribe(Action<object> listener) =>
                EventBus.Unsubscribe<T>(listener);

            public void Unsubscribe<TPayload>(Action<object, TPayload> listener) =>
                EventBus.Unsubscribe<T, TPayload>(listener);
        }

        private class EventListenerSubscriptionBuilder<T> : IEventKickStartBuilder
            where T : class
        {
            private readonly Action<object> _listener;
            private bool _kickStart;
            
            internal EventListenerSubscriptionBuilder(Action<object> listener) =>
                _listener = listener;
            
            public IEventSubscribeBuilder WithKickStart()
            {
                EventBus.SendEventCache<T>(_listener);
                return this;
            }
            
            public void Subscribe() =>
                EventBus.Subscribe<T>(_listener);
        }

        private class EventListenerSubscriptionBuilder<T, TPayload> : IEventKickStartBuilder
            where T : class
        {
            private readonly Action<object, TPayload> _listener;
            private bool _kickStart;
            
            internal EventListenerSubscriptionBuilder(Action<object, TPayload> listener) =>
                _listener = listener;

            public IEventSubscribeBuilder WithKickStart()
            {
                EventBus.SendEventCache<T, TPayload>(_listener);
                return this;
            }

            public void Subscribe() =>
                EventBus.Subscribe<T, TPayload>(_listener);
        }
    }
}