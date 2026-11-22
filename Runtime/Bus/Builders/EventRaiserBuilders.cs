using System;
using AceLand.EventDriven.Bus.Core;
using UnityEngine;

namespace AceLand.EventDriven.Bus.Builders
{
    public static class EventRaiserBuilders
    {
        /// <summary>
        /// Covariant raise entry point returned by <see cref="EventBus.Event{TEvent}"/>.
        /// Covariance keeps the fluent API shaped at compile time: <c>WithData</c> only
        /// surfaces for <c>IEvent&lt;TData&gt;</c> events, while <c>Raise</c>/<c>WithCache</c>
        /// surface for <c>IEvent</c> events. The concrete implementation is a per-closed-type
        /// singleton, so obtaining a raiser allocates nothing after the first use.
        /// </summary>
        public interface IEventRaiser<out TEvent> where TEvent : IBusEvent
        {
            Type EventType { get; }
        }

        // Stateless, per-closed-generic-type singleton. Reference type is required for the
        // covariant IEventRaiser<out TEvent>; keeping a single shared instance means
        // EventBus.Event<T>() performs zero heap allocation on the raise hot path.
        internal sealed class EventBusRaiser<TEvent> : IEventRaiser<TEvent>
            where TEvent : IBusEvent
        {
            internal static readonly EventBusRaiser<TEvent> Shared = new();

            private EventBusRaiser() { }

            public Type EventType => typeof(TEvent);
        }

        /// <summary>
        /// Builder returned after <c>WithData</c>. Calling <see cref="Raise"/> directly
        /// raises the event WITHOUT recording a cache. Add <c>WithCache()</c> to record one.
        /// A readonly struct: the WithData → Raise chain allocates nothing.
        /// </summary>
        public readonly struct EventBusRaiserWithData<TPayload>
        {
            private readonly Type _eventType;
            private readonly TPayload _payload;

            internal EventBusRaiserWithData(Type eventType, TPayload payload)
            {
                _eventType = eventType;
                _payload = payload;
            }

            public void Raise() =>
                EventBus.RaiseEvent(_eventType, _payload, null);

            public EventBusCacheBuilderWithData<TPayload> WithCache() =>
                new(_eventType, _payload, CacheSpec.Persistent);
        }

        /// <summary>
        /// Builder returned after <c>WithCache()</c> on a no-data event. Calling
        /// <see cref="Raise"/> records a persistent cache. Chain <c>WithBinding(...)</c> /
        /// <c>WithBindingScene()</c> to bind the cache lifetime before raising.
        /// </summary>
        public readonly struct EventBusCacheBuilder
        {
            private readonly Type _eventType;
            private readonly CacheSpec _spec;

            internal EventBusCacheBuilder(Type eventType, CacheSpec spec)
            {
                _eventType = eventType;
                _spec = spec;
            }

            public void Raise() =>
                EventBus.RaiseEvent(_eventType, _spec);

            public EventBusCacheBuilder WithBinding(GameObject target) =>
                new(_eventType, CacheSpec.ForGameObject(target));

            public EventBusCacheBuilder WithBindingScene() =>
                new(_eventType, CacheSpec.ForActiveScene());
        }

        /// <summary>
        /// Builder returned after <c>WithData(...).WithCache()</c>. Calling <see cref="Raise"/>
        /// records a persistent cache carrying the payload. Chain <c>WithBinding(...)</c> /
        /// <c>WithBindingScene()</c> to bind the cache lifetime before raising.
        /// </summary>
        public readonly struct EventBusCacheBuilderWithData<TPayload>
        {
            private readonly Type _eventType;
            private readonly TPayload _payload;
            private readonly CacheSpec _spec;

            internal EventBusCacheBuilderWithData(Type eventType, TPayload payload, CacheSpec spec)
            {
                _eventType = eventType;
                _payload = payload;
                _spec = spec;
            }

            public void Raise() =>
                EventBus.RaiseEvent(_eventType, _payload, _spec);

            public EventBusCacheBuilderWithData<TPayload> WithBinding(GameObject target) =>
                new(_eventType, _payload, CacheSpec.ForGameObject(target));

            public EventBusCacheBuilderWithData<TPayload> WithBindingScene() =>
                new(_eventType, _payload, CacheSpec.ForActiveScene());
        }
    }
}
