namespace AceLand.EventDriven.Bus
{
    public static class EventRaiserBuilders
    {
        public interface IEventRaiserPayloadBuilder : IEventRaiserRaiseBuilder
        {
            IEventRaiserRaiseBuilder WithData<TPayload>(TPayload data);
        }
        
        public interface IEventRaiserRaiseBuilder
        {
            void Raise();
        }

        internal class EventBusRaiserBuilder<T> : IEventRaiserPayloadBuilder
            where T : IEvent
        {
            private readonly object _sender;
            
            public EventBusRaiserBuilder(object sender)
            {
                _sender = sender;
            }

            public IEventRaiserRaiseBuilder WithData<TPayload>(TPayload data) =>
                new EventBusRaiserBuilder<T, TPayload>(_sender, data);
            
            public void Raise() =>
                EventBus.RaiseEvent<T>(_sender);
        }

        private class EventBusRaiserBuilder<T, TPayload> : IEventRaiserRaiseBuilder
            where T : IEvent
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