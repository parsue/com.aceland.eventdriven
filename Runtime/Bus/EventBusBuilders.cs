using System;
using ZLinq;

namespace AceLand.EventDriven.Bus
{
    public static class EventBusBuilders
    {
        public interface IEventBusObjBuilder
        {
            IEventKickStartInstanceBuilder Listen();
            void Unlisten();
        }
        
        public interface IEventBusBuilder
        {
            EventRaiserBuilders.IEventRaiserPayloadBuilder WithSender(object sender);
        }

        public interface IEventKickStartInstanceBuilder
        {
            IEventKickStartInstanceBuilder KickStart();
            void Done();
        }

        internal class EventBusBuilder<TEvent> : IEventBusBuilder, IEventBusObjBuilder
            where TEvent : IEvent
        {
            private readonly object _instanceOrNull;

            public EventBusBuilder(object instanceOrNull)
            {
                _instanceOrNull = instanceOrNull;
            }

            public EventRaiserBuilders.IEventRaiserPayloadBuilder WithSender(object sender) =>
                new EventRaiserBuilders.EventBusRaiserBuilder<TEvent>(sender);

            public IEventKickStartInstanceBuilder Listen()
            {
                if (_instanceOrNull == null)
                    throw new InvalidOperationException("Listen() requires an instance. Use Event<TEvent>(instance).");

                EventBus.SubscribeInstance(typeof(TEvent), _instanceOrNull);

                return new KickStartInstanceBuilder(() => { /* no-op */ }, () =>
                {
                    EventBus.KickStartInstance(typeof(TEvent), _instanceOrNull);
                });
            }

            public void Unlisten()
            {
                if (_instanceOrNull == null)
                    throw new InvalidOperationException("Unlisten() requires an instance. Use Event<TEvent>(instance).");

                EventBus.UnsubscribeInstance(typeof(TEvent), _instanceOrNull);
            }

            private sealed class KickStartInstanceBuilder : IEventKickStartInstanceBuilder
            {
                private readonly Action _onDone;
                private readonly Action _onKickStart;

                public KickStartInstanceBuilder(Action onDone, Action onKickStart)
                {
                    _onDone = onDone ?? (() => { });
                    _onKickStart = onKickStart ?? (() => { });
                }

                public IEventKickStartInstanceBuilder KickStart()
                {
                    _onKickStart();
                    return this;
                }

                public void Done() => _onDone();
            }
        }

        internal class MultiEventBusBuilder : IEventBusBuilder, IEventBusObjBuilder
        {
            private readonly object _instance;
            private readonly Type[] _eventInterfaces;

            public MultiEventBusBuilder(object instance)
            {
                _instance = instance ?? throw new ArgumentNullException(nameof(instance));
                _eventInterfaces = _instance.GetType()
                    .GetInterfaces()
                    .AsValueEnumerable()
                    .Where(i => i != typeof(IEvent) && typeof(IEvent).IsAssignableFrom(i) && i.IsInterface)
                    .ToArray();
            }

            public EventRaiserBuilders.IEventRaiserPayloadBuilder WithSender(object sender) =>
                throw new InvalidOperationException("WithSender is not valid for Event(instance). Use Event<TEvent>().WithSender.");

            public EventListenerBuilders.IEventKickStartBuilder WithListener(Action<object> listener) =>
                throw new InvalidOperationException("WithListener(Action) is not valid for Event(instance). Use Event<TEvent>(instance) or Event<TEvent>().WithListener.");

            public EventListenerBuilders.IEventKickStartBuilder WithListener<TPayload>(Action<object, TPayload> listener) =>
                throw new InvalidOperationException("WithListener(Action<object,TPayload>) is not valid for Event(instance). Use Event<TEvent>(instance).");

            public IEventKickStartInstanceBuilder Listen()
            {
                foreach (var ev in _eventInterfaces)
                {
                    EventBus.SubscribeInstance(ev, _instance);
                }

                return new KickStartInstanceBuilder(() => { /* no-op */ }, () =>
                {
                    foreach (var ev in _eventInterfaces)
                    {
                        EventBus.KickStartInstance(ev, _instance);
                    }
                });
            }

            public void Unlisten()
            {
                EventBus.UnsubscribeAllForInstance(_instance);
            }

            public void Unlisten(Action<object> listener) =>
                throw new InvalidOperationException("Unlisten(Action) is not valid for Event(instance).");

            public void Unlisten<TPayload>(Action<object, TPayload> listener) =>
                throw new InvalidOperationException("Unlisten(Action<object,TPayload>) is not valid for Event(instance).");

            private sealed class KickStartInstanceBuilder : IEventKickStartInstanceBuilder
            {
                private readonly Action _onDone;
                private readonly Action _onKickStart;

                public KickStartInstanceBuilder(Action onDone, Action onKickStart)
                {
                    _onDone = onDone ?? (() => { });
                    _onKickStart = onKickStart ?? (() => { });
                }

                public IEventKickStartInstanceBuilder KickStart()
                {
                    _onKickStart();
                    return this;
                }

                public void Done() => _onDone();
            }
        }
    }
}