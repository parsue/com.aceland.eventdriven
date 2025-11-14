using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ZLinq;

namespace AceLand.EventDriven.Bus
{
    public static class EventBus
    {
        private static readonly object @lock = new();

        private static readonly Dictionary<Type, EventSignature> signatures = new();
        private static readonly Dictionary<Type, Delegate> listeners = new();
        private static readonly Dictionary<Type, Dictionary<object, Delegate>> instanceDelegates = new();
        private static readonly Dictionary<Type, EventCache> eventCache = new();

        public static EventBusBuilders.IEventBusBuilder Event<TEvent>() where TEvent : IEvent
        {
            EnsureIsEventInterface(typeof(TEvent));
            return new EventBusBuilders.EventBusBuilder<TEvent>(null);
        }

        public static EventBusBuilders.IEventBusObjBuilder Event(object listenerInstance)
        {
            if (listenerInstance == null) throw new ArgumentNullException(nameof(listenerInstance));
            return new EventBusBuilders.MultiEventBusBuilder(listenerInstance);
        }

        public static EventBusBuilders.IEventBusObjBuilder Event<TEvent>(object listenerInstance) where TEvent : IEvent
        {
            EnsureIsEventInterface(typeof(TEvent));
            if (listenerInstance == null) throw new ArgumentNullException(nameof(listenerInstance));
            return new EventBusBuilders.EventBusBuilder<TEvent>(listenerInstance);
        }

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
                foreach (var kv in instanceDelegates)
                {
                    var eventType = kv.Key;
                    var map = kv.Value;
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
                        var mi = typeof(EventBus).GetMethod(nameof(InvokeWithObjectPayload), BindingFlags.NonPublic | BindingFlags.Static);
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

        public static void ClearCache<TEvent>() where TEvent : IEvent
        {
            lock (@lock) eventCache.Remove(typeof(TEvent));
        }

        public static void ClearAllCache()
        {
            lock (@lock) eventCache.Clear();
        }

        internal static void BootstrapRegister(IEnumerable<Type> eventInterfaces)
        {
            lock (@lock)
            {
                signatures.Clear();
                listeners.Clear();
                instanceDelegates.Clear();
                eventCache.Clear();

                foreach (var it in eventInterfaces)
                {
                    var sig = BuildSignature(it);
                    if (sig == null) continue;
                    signatures[it] = sig;
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
                : throw new InvalidOperationException($"Event interface {eventInterface.Name} is not registered. Ensure bootstrap ran.");
        }

        private static EventSignature BuildSignature(Type eventInterface)
        {
            EnsureIsEventInterface(eventInterface);

            var methods = eventInterface.GetMethods();
            if (methods.Length != 1)
            {
                Debug.LogError($"Event interface {eventInterface.Name} must declare exactly one method.");
                return null;
            }

            var m = methods[0];
            if (m.ReturnType != typeof(void))
            {
                Debug.LogError($"Event method {eventInterface.Name}.{m.Name} must return void.");
                return null;
            }

            var pars = m.GetParameters();
            switch (pars.Length)
            {
                case 1 when pars[0].ParameterType != typeof(object):
                    Debug.LogError($"First parameter of {eventInterface.Name}.{m.Name} must be object sender.");
                    return null;
                case 1:
                    return new EventSignature(eventInterface, m, EventSignatureKind.NoPayload, null);
                case 2 when pars[0].ParameterType != typeof(object):
                    Debug.LogError($"First parameter of {eventInterface.Name}.{m.Name} must be object sender.");
                    return null;
                case 2:
                {
                    var payloadType = pars[1].ParameterType;
                    return new EventSignature(eventInterface, m, EventSignatureKind.SinglePayload, payloadType);
                }
                default:
                    Debug.LogError($"Event method {eventInterface.Name}.{m.Name} must have 1 or 2 parameters.");
                    return null;
            }
        }

        private static Delegate BindInstanceDelegate(EventSignature sig, object instance)
        {
            if (!sig.EventInterfaceType.IsAssignableFrom(instance.GetType()))
                throw new InvalidOperationException($"{instance.GetType().Name} does not implement {sig.EventInterfaceType.Name}.");

            var targetMethod = ResolveImplementationMethod(instance.GetType(), sig.EventInterfaceType, sig.Method);

            if (sig.Kind == EventSignatureKind.NoPayload)
            {
                return (Action<object>)((sender) => targetMethod.Invoke(instance, new[] { sender }));
            }
            else
            {
                var helper = typeof(EventBus).GetMethod(nameof(CreateTwoParamAction), BindingFlags.NonPublic | BindingFlags.Static);
                if (helper == null) 
                    throw new InvalidOperationException($"{instance.GetType().Name} does not implement {sig.EventInterfaceType.Name}.");
                var closed = helper.MakeGenericMethod(sig.PayloadTypeOrNull);
                return (Delegate)closed.Invoke(null, new[] { instance, targetMethod });
            }
        }

        private static MethodInfo ResolveImplementationMethod(Type concreteType, Type interfaceType, MethodInfo interfaceMethod)
        {
            var map = concreteType.GetInterfaceMap(interfaceType);
            for (var i = 0; i < map.InterfaceMethods.Length; i++)
            {
                if (map.InterfaceMethods[i] == interfaceMethod)
                    return map.TargetMethods[i];
            }

            var name = interfaceMethod.Name;
            var pars = interfaceMethod.GetParameters().AsValueEnumerable().Select(p => p.ParameterType).ToArray();
            var impl = concreteType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, pars, null);
            return impl == null 
                ? throw new MissingMethodException($"Cannot find implementation for {interfaceType.Name}.{name} on {concreteType.Name}.") 
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

    internal sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();
        public new bool Equals(object x, object y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}