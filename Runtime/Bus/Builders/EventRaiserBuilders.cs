using System;
using AceLand.EventDriven.Bus.Core;
using UnityEngine;

namespace AceLand.EventDriven.Bus.Builders
{
    public static class EventRaiserBuilders
    {
        public interface IEventRaiser<out TEvent> where TEvent : IBusEvent
        {
            Type EventType { get; }
        }

        /// <summary>
        /// Builder returned after <c>WithData</c>. Calling <see cref="Raise"/> directly
        /// raises the event WITHOUT recording a cache. Add <c>WithCache()</c> to record one.
        /// </summary>
        public interface IEventRaiserRaiseBuilder
        {
            void Raise();
            IEventRaiserCacheBuilder WithCache();
        }

        /// <summary>
        /// Builder returned after <c>WithCache()</c>. Calling <see cref="Raise"/> records a
        /// persistent cache. Chain <c>WithBinding(...)</c> / <c>WithBindingScene()</c> to bind
        /// the cache lifetime to a GameObject or the active Scene before raising.
        /// </summary>
        public interface IEventRaiserCacheBuilder
        {
            void Raise();
            IEventRaiserCacheBuilder WithBinding(GameObject target);
            IEventRaiserCacheBuilder WithBindingScene();
        }

        internal interface IEventRaiserInternal
        {
            void InternalRaise(CacheSpec? spec);
            IEventRaiserCacheBuilder InternalWithCache();
            IEventRaiserRaiseBuilder InternalRaiseWithData<TPayload>(TPayload data);
        }

        internal class EventBusRaiser<TEvent> : IEventRaiser<TEvent>, IEventRaiserInternal
            where TEvent : IBusEvent
        {
            public Type EventType => typeof(TEvent);

            public void InternalRaise(CacheSpec? spec) =>
                EventBus.RaiseEvent(typeof(TEvent), spec);

            public IEventRaiserCacheBuilder InternalWithCache() =>
                new EventBusCacheBuilder(typeof(TEvent), CacheSpec.Persistent);

            public IEventRaiserRaiseBuilder InternalRaiseWithData<TPayload>(TPayload data) =>
                new EventBusRaiserWithData<TPayload>(typeof(TEvent), data);
        }

        internal sealed class EventBusCacheBuilder : IEventRaiserCacheBuilder
        {
            private readonly Type _eventType;
            private readonly CacheSpec _spec;

            public EventBusCacheBuilder(Type eventType, CacheSpec spec)
            {
                _eventType = eventType;
                _spec = spec;
            }

            public void Raise() =>
                EventBus.RaiseEvent(_eventType, _spec);

            public IEventRaiserCacheBuilder WithBinding(GameObject target) =>
                new EventBusCacheBuilder(_eventType, CacheSpec.ForGameObject(target));

            public IEventRaiserCacheBuilder WithBindingScene() =>
                new EventBusCacheBuilder(_eventType, CacheSpec.ForActiveScene());
        }

        internal sealed class EventBusRaiserWithData<TPayload> : IEventRaiserRaiseBuilder
        {
            private readonly Type _eventType;
            private readonly TPayload _payload;

            public EventBusRaiserWithData(Type eventType, TPayload payload)
            {
                _eventType = eventType;
                _payload = payload;
            }

            public void Raise() =>
                EventBus.RaiseEvent(_eventType, _payload, null);

            public IEventRaiserCacheBuilder WithCache() =>
                new EventBusCacheBuilderWithData<TPayload>(_eventType, _payload, CacheSpec.Persistent);
        }

        internal sealed class EventBusCacheBuilderWithData<TPayload> : IEventRaiserCacheBuilder
        {
            private readonly Type _eventType;
            private readonly TPayload _payload;
            private readonly CacheSpec _spec;

            public EventBusCacheBuilderWithData(Type eventType, TPayload payload, CacheSpec spec)
            {
                _eventType = eventType;
                _payload = payload;
                _spec = spec;
            }

            public void Raise() =>
                EventBus.RaiseEvent(_eventType, _payload, _spec);

            public IEventRaiserCacheBuilder WithBinding(GameObject target) =>
                new EventBusCacheBuilderWithData<TPayload>(_eventType, _payload, CacheSpec.ForGameObject(target));

            public IEventRaiserCacheBuilder WithBindingScene() =>
                new EventBusCacheBuilderWithData<TPayload>(_eventType, _payload, CacheSpec.ForActiveScene());
        }
    }
}
