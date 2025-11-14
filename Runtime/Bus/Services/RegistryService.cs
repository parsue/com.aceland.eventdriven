using System;
using System.Collections.Generic;
using System.Reflection;
using AceLand.EventDriven.Bus.Core;
using ZLinq;

namespace AceLand.EventDriven.Bus.Services
{
    internal sealed class RegistryService
    {
        public static RegistryService Build(SignatureService signatures, CacheService cache) => new(signatures, cache);
        private RegistryService(SignatureService signatures, CacheService cache)
        {
            _signatures = signatures ?? throw new ArgumentNullException(nameof(signatures));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }
        
        private readonly object _lock = new();
        private readonly Dictionary<Type, Delegate> _listeners = new();
        private readonly Dictionary<Type, Dictionary<object, Delegate>> _instanceDelegates = new();

        private readonly SignatureService _signatures;
        private readonly CacheService _cache;

        public void BootstrapReset()
        {
            lock (_lock)
            {
                _listeners.Clear();
                _instanceDelegates.Clear();
            }
            _cache.InternalClearAll();
        }

        public void SubscribeInstance(Type eventType, object instance)
        {
            EventBus.EnsureIsEventInterface(eventType);
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var sig = _signatures.GetOrThrowSignature(eventType);
            var del = BindInstanceDelegate(sig, instance);

            lock (_lock)
            {
                if (_listeners.TryGetValue(eventType, out var existing))
                    _listeners[eventType] = Delegate.Combine(existing, del);
                else
                    _listeners[eventType] = del;

                if (!_instanceDelegates.TryGetValue(eventType, out var map))
                {
                    map = new Dictionary<object, Delegate>(ReferenceEqualityComparer.Instance);
                    _instanceDelegates[eventType] = map;
                }

                map[instance] = Delegate.Combine(map.GetValueOrDefault(instance), del);
            }
        }

        public void UnsubscribeInstance(Type eventType, object instance)
        {
            EventBus.EnsureIsEventInterface(eventType);
            if (instance == null) return;

            lock (_lock)
            {
                if (!_instanceDelegates.TryGetValue(eventType, out var map)) return;
                if (!map.TryGetValue(instance, out var instanceDel)) return;

                if (_listeners.TryGetValue(eventType, out var master))
                {
                    master = Delegate.Remove(master, instanceDel);
                    if (master == null) _listeners.Remove(eventType);
                    else _listeners[eventType] = master;
                }

                map.Remove(instance);
                if (map.Count == 0) _instanceDelegates.Remove(eventType);
            }
        }

        public void UnsubscribeAllForInstance(object instance)
        {
            if (instance == null) return;

            lock (_lock)
            {
                var toUpdate = new List<Type>();
                foreach (var kvp in _instanceDelegates)
                {
                    var eventType = kvp.Key;
                    var map = kvp.Value;
                    if (!map.TryGetValue(instance, out var instanceDel)) continue;

                    if (_listeners.TryGetValue(eventType, out var master))
                    {
                        master = Delegate.Remove(master, instanceDel);
                        if (master == null) _listeners.Remove(eventType);
                        else _listeners[eventType] = master;
                    }

                    map.Remove(instance);
                    if (map.Count == 0) toUpdate.Add(eventType);
                }

                foreach (var et in toUpdate) _instanceDelegates.Remove(et);
            }
        }

        public void SubscribeDelegate(Type eventType, Action<object> listener)
        {
            EventBus.EnsureIsEventInterface(eventType);
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            lock (_lock)
            {
                if (_listeners.TryGetValue(eventType, out var existing))
                    _listeners[eventType] = Delegate.Combine(existing, listener);
                else
                    _listeners[eventType] = listener;
            }
        }

        public void SubscribeDelegate<TPayload>(Type eventType, Action<object, TPayload> listener)
        {
            EventBus.EnsureIsEventInterface(eventType);
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            lock (_lock)
            {
                if (_listeners.TryGetValue(eventType, out var existing))
                    _listeners[eventType] = Delegate.Combine(existing, listener);
                else
                    _listeners[eventType] = listener;
            }
        }

        public void KickStartInstance(Type eventType, object instance)
        {
            if (eventType == null) throw new ArgumentNullException(nameof(eventType));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var sig = _signatures.GetOrThrowSignature(eventType);
            var del = BindInstanceDelegate(sig, instance);
            SendCacheToDelegate(sig, del);
        }

        public void SendCacheToDelegate(Type eventType, Action<object> listener)
        {
            _cache.WithCache(eventType, cache =>
            {
                (listener)?.Invoke(cache.Sender);
            });
        }

        public void SendCacheToDelegate<TPayload>(Type eventType, Action<object, TPayload> listener)
        {
            _cache.WithCache(eventType, cache =>
            {
                listener?.Invoke(cache.Sender, (TPayload)cache.EventData);
            });
        }

        public void RaiseEvent(Type eventType, object sender)
        {
            EventBus.EnsureIsEventInterface(eventType);
            _cache.SetCache(eventType, new EventCache(sender, null));
            Delegate del;
            lock (_lock)
            {
                _listeners.TryGetValue(eventType, out del);
            }
            (del as Action<object>)?.Invoke(sender);
        }

        public void RaiseEvent<TPayload>(Type eventType, object sender, TPayload payload)
        {
            EventBus.EnsureIsEventInterface(eventType);
            _cache.SetCache(eventType, new EventCache(sender, payload));
            Delegate del;
            lock (_lock)
            {
                _listeners.TryGetValue(eventType, out del);
            }
            (del as Action<object, TPayload>)?.Invoke(sender, payload);
        }

        private Delegate BindInstanceDelegate(EventSignature sig, object instance)
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
                var helper = typeof(RegistryService).GetMethod(nameof(CreateTwoParamAction),
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

        private void SendCacheToDelegate(EventSignature sig, Delegate del)
        {
            _cache.WithCache(sig.EventInterfaceType, cache =>
            {
                switch (sig.Kind)
                {
                    case EventSignatureKind.NoPayload:
                        (del as Action<object>)?.Invoke(cache.Sender);
                        break;
                    case EventSignatureKind.SinglePayload:
                        InvokeWithObjectPayload(sig.PayloadTypeOrNull, del, cache.Sender, cache.EventData);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        private static void InvokeWithObjectPayload(Type payloadType, Delegate del, object sender, object payload)
        {
            var mi = typeof(RegistryService).GetMethod(nameof(InvokeGenericPayload),
                BindingFlags.NonPublic | BindingFlags.Static);
            var gmi = mi!.MakeGenericMethod(payloadType);
            gmi.Invoke(null, new[] { del, sender, payload });
        }

        private static void InvokeGenericPayload<TPayload>(Delegate del, object sender, object payload)
        {
            (del as Action<object, TPayload>)?.Invoke(sender, (TPayload)payload);
        }
    }
}