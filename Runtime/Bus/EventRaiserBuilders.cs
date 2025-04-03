namespace AceLand.EventDriven.Bus
{
    public static class EventRaiserBuilders
    {
        public interface IEventBusRaiserBuilder : IEventRaiserRaiseBuilder
        {
            IEventRaiserRaiseBuilder WithData<TPayload>(TPayload data);
        }

        public interface IEventRaiserRaiseBuilder
        {
            void Raise();
        }

        internal class EventBusEventRaiserBuilder<T> : IEventBusRaiserBuilder
            where T : class
        {
            private readonly object _sender;
            
            internal EventBusEventRaiserBuilder(object sender) =>
                _sender = sender;

            public IEventRaiserRaiseBuilder WithData<TPayload>(TPayload data) =>
                new EventBusRaiserBuilder<T, TPayload>(_sender, data);

            public void Raise() =>
                EventBus.RaiseEvent<T>(_sender);
        }

        private class EventBusRaiserBuilder<T, TPayload> : IEventRaiserRaiseBuilder
            where T : class
        {
            private readonly object _sender;
            private readonly TPayload _payload;
            
            public EventBusRaiserBuilder(object sender, TPayload payload)
            {
                _sender = sender;
                _payload = payload;
            }
            
            public void Raise() =>
                EventBus.RaiseEvent<T, TPayload>(_sender, _payload);
        }
    }
}