using System;
using System.Collections.Generic;
using System.Reflection;
using ZLinq;

namespace AceLand.EventDriven.Bus
{
    public static partial class EventBus
    {
        private static readonly object @lock = new();
        private static readonly Dictionary<Type, Delegate> listeners = new();
        private static readonly Dictionary<Type, Dictionary<object, Delegate>> instanceDelegates = new();

        internal static void SubscribeInstance(Type eventType, object instance)
        {
            EnsureIsEventInterface(eventType);
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var sig = GetOrThrowSignature(eventType);
            var del = BindInstanceDelegate(sig, instance);

            lock (@lock)
            {
                if (listeners.TryGetValue(eventType, out var existing))
                    listeners[eventType] = Delegate.Combine(existing, del);
                else
                    listeners[eventType] = del;

                if (!instanceDelegates.TryGetValue(eventType, out var map))
                {
                    map = new Dictionary<object, Delegate>(ReferenceEqualityComparer.Instance);
                    instanceDelegates[eventType] = map;
                }

                map[instance] = Delegate.Combine(map.GetValueOrDefault(instance), del);
            }
        }

        internal static void UnsubscribeInstance(Type eventType, object instance)
        {
            EnsureIsEventInterface(eventType);
            if (instance == null) return;

            lock (@lock)
            {
                if (!instanceDelegates.TryGetValue(eventType, out var map)) return;
                if (!map.TryGetValue(instance, out var instanceDel)) return;

                if (listeners.TryGetValue(eventType, out var master))
                {
                    master = Delegate.Remove(master, instanceDel);
                    if (master == null) listeners.Remove(eventType);
                    else listeners[eventType] = master;
                }

                map.Remove(instance);
                if (map.Count == 0) instanceDelegates.Remove(eventType);
            }
        }

        internal static void UnsubscribeAllForInstance(object instance)
        {
            if (instance == null) return;

            lock (@lock)
            {
                var toUpdate = new List<Type>();
                foreach (var (eventType, map) in instanceDelegates)
                {
                    if (!map.TryGetValue(instance, out var instanceDel)) continue;

                    if (listeners.TryGetValue(eventType, out var master))
                    {
                        master = Delegate.Remove(master, instanceDel);
                        if (master == null) listeners.Remove(eventType);
                        else listeners[eventType] = master;
                    }

                    map.Remove(instance);
                    if (map.Count == 0) toUpdate.Add(eventType);
                }

                foreach (var et in toUpdate) instanceDelegates.Remove(et);
            }
        }

        internal static void SubscribeDelegate<TEvent>(Action<object> listener)
            where TEvent : IEvent
        {
            EnsureIsEventInterface(typeof(TEvent));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            lock (@lock)
            {
                if (listeners.TryGetValue(typeof(TEvent), out var existing))
                    listeners[typeof(TEvent)] = Delegate.Combine(existing, listener);
                else
                    listeners[typeof(TEvent)] = listener;
            }
        }

        internal static void SubscribeDelegate<TEvent, TPayload>(Action<object, TPayload> listener)
            where TEvent : IEvent
        {
            EnsureIsEventInterface(typeof(TEvent));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            lock (@lock)
            {
                if (listeners.TryGetValue(typeof(TEvent), out var existing))
                    listeners[typeof(TEvent)] = Delegate.Combine(existing, listener);
                else
                    listeners[typeof(TEvent)] = listener;
            }
        }

        internal static void KickStartInstance(Type eventType, object instance)
        {
            if (eventType == null) throw new ArgumentNullException(nameof(eventType));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var sig = GetOrThrowSignature(eventType);
            var del = BindInstanceDelegate(sig, instance);
            SendCacheToDelegate(sig, del);
        }

        private static void SendCacheToDelegate<TEvent>(Action<object> listener)
            where TEvent : IEvent
        {
            var t = typeof(TEvent);
            lock (@lock)
            {
                if (!eventCache.TryGetValue(t, out var cache)) return;
                listener(cache.Sender);
            }
        }

        private static void SendCacheToDelegate<TEvent, TPayload>(Action<object, TPayload> listener)
            where TEvent : IEvent
        {
            var t = typeof(TEvent);
            lock (@lock)
            {
                if (!eventCache.TryGetValue(t, out var cache)) return;
                listener(cache.Sender, (TPayload)cache.EventData);
            }
        }

        private static void SendCacheToDelegate(EventSignature sig, Delegate del)
        {
            lock (@lock)
            {
                if (!eventCache.TryGetValue(sig.EventInterfaceType, out var cache)) return;
                switch (sig.Kind)
                {
                    case EventSignatureKind.NoPayload:
                        (del as Action<object>)?.Invoke(cache.Sender);
                        break;
                    case EventSignatureKind.SinglePayload:
                        var mi = typeof(EventBus).GetMethod(nameof(InvokeWithObjectPayload),
                            BindingFlags.NonPublic | BindingFlags.Static);
                        if (mi == null) break;
                        var gmi = mi.MakeGenericMethod(sig.PayloadTypeOrNull);
                        gmi.Invoke(null, new[] { del, cache.Sender, cache.EventData });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void InvokeWithObjectPayload<TPayload>(Delegate del, object sender, object payload)
        {
            (del as Action<object, TPayload>)?.Invoke(sender, (TPayload)payload);
        }

        internal static void RaiseEvent<TEvent>(object sender)
            where TEvent : IEvent
        {
            EnsureIsEventInterface(typeof(TEvent));
            lock (@lock)
            {
                eventCache[typeof(TEvent)] = new EventCache(sender, null);
                if (listeners.TryGetValue(typeof(TEvent), out var del))
                {
                    (del as Action<object>)?.Invoke(sender);
                }
            }
        }

        internal static void RaiseEvent<TEvent, TPayload>(object sender, TPayload payload)
            where TEvent : IEvent
        {
            EnsureIsEventInterface(typeof(TEvent));
            lock (@lock)
            {
                eventCache[typeof(TEvent)] = new EventCache(sender, payload);
                if (listeners.TryGetValue(typeof(TEvent), out var del))
                {
                    (del as Action<object, TPayload>)?.Invoke(sender, payload);
                }
            }
        }

        private static void EnsureIsEventInterface(Type t)
        {
            if (t is not { IsInterface: true } || !typeof(IEvent).IsAssignableFrom(t))
                throw new ArgumentException($"Event type {t?.Name} must be an interface implementing IEvent.");
        }

        private static EventSignature GetOrThrowSignature(Type eventInterface)
        {
            return signatures.TryGetValue(eventInterface, out var sig)
                ? sig
                : throw new InvalidOperationException(
                    $"Event interface {eventInterface.Name} is not registered. Ensure bootstrap ran.");
        }

        private static Delegate BindInstanceDelegate(EventSignature sig, object instance)
        {
            if (!sig.EventInterfaceType.IsAssignableFrom(instance.GetType()))
                throw new InvalidOperationException(
                    $"{instance.GetType().Name} does not implement {sig.EventInterfaceType.Name}.");

            var targetMethod = ResolveImplementationMethod(instance.GetType(), sig.EventInterfaceType, sig.Method);

            if (sig.Kind == EventSignatureKind.NoPayload)
            {
                return (Action<object>)((sender) => targetMethod.Invoke(instance, new[] { sender }));
            }
            else
            {
                var helper = typeof(EventBus).GetMethod(nameof(CreateTwoParamAction),
                    BindingFlags.NonPublic | BindingFlags.Static);
                if (helper == null)
                    throw new InvalidOperationException(
                        $"{instance.GetType().Name} does not implement {sig.EventInterfaceType.Name}.");
                var closed = helper.MakeGenericMethod(sig.PayloadTypeOrNull);
                return (Delegate)closed.Invoke(null, new[] { instance, targetMethod });
            }
        }

        private static MethodInfo ResolveImplementationMethod(Type concreteType, Type interfaceType,
            MethodInfo interfaceMethod)
        {
            var map = concreteType.GetInterfaceMap(interfaceType);
            for (var i = 0; i < map.InterfaceMethods.Length; i++)
            {
                if (map.InterfaceMethods[i] == interfaceMethod)
                    return map.TargetMethods[i];
            }

            var name = interfaceMethod.Name;
            var pars = interfaceMethod.GetParameters().AsValueEnumerable().Select(p => p.ParameterType).ToArray();
            var impl = concreteType.GetMethod(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, pars, null);
            return impl == null
                ? throw new MissingMethodException(
                    $"Cannot find implementation for {interfaceType.Name}.{name} on {concreteType.Name}.")
                : impl;
        }

        private static Delegate CreateTwoParamAction<TPayload>(object instance, MethodInfo target)
        {
            return (Action<object, TPayload>)Action;
            void Action(object sender, TPayload payload) => target.Invoke(instance, new[] { sender, payload });
        }

        internal static void SendEventCache<TEvent>(Action<object> listener) where TEvent : IEvent
        {
            SendCacheToDelegate<TEvent>(listener);
        }

        internal static void SendEventCache<TEvent, TPayload>(Action<object, TPayload> listener) where TEvent : IEvent
        {
            SendCacheToDelegate<TEvent, TPayload>(listener);
        }
    }
}