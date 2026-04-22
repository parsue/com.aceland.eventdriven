using System;
using ZLinq;

namespace AceLand.EventDriven.Bus.Builders
{
    public static class EventBusBuilders
    {
        public interface IEventBusObjBuilder
        {
            IEventKickStartInstanceBuilder Listen();
            void Unlisten();
        }

        public interface IEventKickStartInstanceBuilder
        {
            IEventKickStartInstanceBuilder KickStart();
            void Done();
        }

        internal sealed class KickStartInstanceBuilder : IEventKickStartInstanceBuilder
        {
            private readonly Action _onKickStart;

            public KickStartInstanceBuilder(Action onKickStart)
            {
                _onKickStart = onKickStart ?? (() => { });
            }

            public IEventKickStartInstanceBuilder KickStart()
            {
                _onKickStart();
                return this;
            }

            public void Done()
            {
            }
        }

        internal class EventBusBuilder<TEvent> : IEventBusObjBuilder
            where TEvent : IBusEvent
        {
            private readonly object _instance;

            public EventBusBuilder(object instance)
            {
                _instance = instance;
            }

            public IEventKickStartInstanceBuilder Listen()
            {
                if (_instance == null)
                    throw new InvalidOperationException("Listen() requires an instance. Use Event<TEvent>(instance).");

                EventBus.SubscribeInstance(typeof(TEvent), _instance);

                return new KickStartInstanceBuilder(() => EventBus.KickStartInstance(typeof(TEvent), _instance));
            }

            public void Unlisten()
            {
                if (_instance == null)
                    throw new InvalidOperationException(
                        "Unlisten() requires an instance. Use Event<TEvent>(instance).");

                EventBus.UnsubscribeInstance(typeof(TEvent), _instance);
            }
        }

        internal class MultiEventBusBuilder : IEventBusObjBuilder
        {
            private readonly object _instance;
            private readonly Type[] _eventInterfaces;

            public MultiEventBusBuilder(object instance)
            {
                _instance = instance ?? throw new ArgumentNullException(nameof(instance));
                _eventInterfaces = _instance.GetType()
                    .GetInterfaces()
                    .AsValueEnumerable()
                    .Where(t =>
                        t.IsInterface &&
                        typeof(IBusEvent).IsAssignableFrom(t) &&
                        t != typeof(IBusEvent) &&
                        t != typeof(IEvent) &&
                        !(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEvent<>)))
                    .ToArray();
            }

            public IEventKickStartInstanceBuilder Listen()
            {
                foreach (var ev in _eventInterfaces)
                    EventBus.SubscribeInstance(ev, _instance);

                return new KickStartInstanceBuilder(() =>
                {
                    foreach (var ev in _eventInterfaces)
                        EventBus.KickStartInstance(ev, _instance);
                });
            }

            public void Unlisten()
            {
                EventBus.UnsubscribeAllForInstance(_instance);
            }
        }
    }
}