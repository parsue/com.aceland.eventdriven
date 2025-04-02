using System;

namespace AceLand.EventDriven.Bus
{
    // Builder for raising an event
    public static class EventBusRaiserBuilder
    {
        public interface IEventRaiserDataBuilder : IEventRaiserRaiseBuilder
        {
            IEventRaiserRaiseBuilder WithData<TPayload>(TPayload data) where TPayload : IEventData;
        }

        public interface IEventRaiserRaiseBuilder
        {
            void Raise();
        }

        internal class EventRaiserBuilder<T> : IEventRaiserDataBuilder
            where T : class
        {
            private readonly object _sender;

            internal EventRaiserBuilder(object sender)
            {
                if (!typeof(T).IsInterface)
                    throw new ArgumentException($"Event type {typeof(T).Name} must be an interface.");
                
                _sender = sender;
            }

            public IEventRaiserRaiseBuilder WithData<TPayload>(TPayload data) where TPayload : IEventData =>
                new EventRaiserBuilder<T, TPayload>(_sender, data);

            public void Raise()
            {
                EventBus.RaiseEvent<T>(_sender);
            }
        }

        internal class EventRaiserBuilder<T, TPayload> : IEventRaiserRaiseBuilder
            where T : class
            where TPayload : IEventData
        {
            private readonly object _sender;
            private readonly TPayload _data;

            internal EventRaiserBuilder(object sender, TPayload data)
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));
                
                _sender = sender;
                _data = data;
            }
            
            public void Raise()
            {
                EventBus.RaiseEvent<T, TPayload>(_sender, _data);
            }
        }
    }
}