using System;
using System.Collections.Generic;
using System.Reflection;
using AceLand.EventDriven.Bus.Core;
using UnityEngine;
using ZLinq;

namespace AceLand.EventDriven.Bus.Services
{
    internal sealed class RegistryService
    {
        public static RegistryService Build(SignatureService signatures, CacheService cache) => new(signatures, cache);

        private RegistryService(SignatureService signatures, CacheService cache)
        {
            _lock = new object();
            _listeners = new Dictionary<Type, Delegate>();
            _instanceDelegates = new Dictionary<Type, Dictionary<object, Delegate>>();
            _signatures = signatures ?? throw new ArgumentNullException(nameof(signatures));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private readonly object _lock;
        private readonly Dictionary<Type, Delegate> _listeners;
        private readonly Dictionary<Type, Dictionary<object, Delegate>> _instanceDelegates;

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
                if (!_instanceDelegates.TryGetValue(eventType, out var map))
                {
                    map = new Dictionary<object, Delegate>(ReferenceEqualityComparer.Instance);
                    _instanceDelegates[eventType] = map;
                }

                if (map.ContainsKey(instance))
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogWarning($"[EventBus] Instance of type {instance.GetType().Name} is already subscribed to {eventType.Name}. Ignoring duplicate subscription.");
#endif
                    return; // Prevent duplicate subscription
                }

                map[instance] = Delegate.Combine(map.GetValueOrDefault(instance), del);

                if (_listeners.TryGetValue(eventType, out var existing))
                    _listeners[eventType] = Delegate.Combine(existing, del);
                else
                    _listeners[eventType] = del;
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

        public void SubscribeDelegate(Type eventType, Action listener)
        {
            EventBus.EnsureIsEventInterface(eventType);
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            lock (_lock)
            {
                if (_listeners.TryGetValue(eventType, out var existing))
                {
                    if (existing != null && existing.GetInvocationList().AsValueEnumerable().Contains(listener))
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogWarning($"[EventBus] Delegate is already subscribed to {eventType.Name}. Ignoring duplicate subscription.");
#endif
                        return; // Prevent duplicate subscription
                    }
                    
                    _listeners[eventType] = Delegate.Combine(existing, listener);
                }
                else
                {
                    _listeners[eventType] = listener;
                }
            }
        }

        public void SubscribeDelegate<TPayload>(Type eventType, Action<TPayload> listener)
        {
            EventBus.EnsureIsEventInterface(eventType);
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            lock (_lock)
            {
                if (_listeners.TryGetValue(eventType, out var existing))
                {
                    if (existing != null && existing.GetInvocationList().AsValueEnumerable().Contains(listener))
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogWarning($"[EventBus] Delegate is already subscribed to {eventType.Name}. Ignoring duplicate subscription.");
#endif
                        return; // Prevent duplicate subscription
                    }
                    
                    _listeners[eventType] = Delegate.Combine(existing, listener);
                }
                else
                {
                    _listeners[eventType] = listener;
                }
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

        public void SendCacheToDelegate(Type eventType, Action listener)
        {
            _cache.WithCache(eventType, _ => { listener?.Invoke(); });
        }

        public void SendCacheToDelegate<TPayload>(Type eventType, Action<TPayload> listener)
        {
            _cache.WithCache(eventType, cache => { listener?.Invoke((TPayload)cache.EventData); });
        }

        public void RaiseEvent(Type eventType)
        {
            EventBus.EnsureIsEventInterface(eventType);
            _cache.SetCache(eventType, new EventCache(null));
            Delegate del;
            lock (_lock)
            {
                _listeners.TryGetValue(eventType, out del);
            }

            (del as Action)?.Invoke();
        }

        public void RaiseEvent<TPayload>(Type eventType, TPayload payload)
        {
            EventBus.EnsureIsEventInterface(eventType);
            _cache.SetCache(eventType, new EventCache(payload));
            Delegate del;
            lock (_lock)
            {
                _listeners.TryGetValue(eventType, out del);
            }

            (del as Action<TPayload>)?.Invoke(payload);
        }

        private static Delegate BindInstanceDelegate(EventSignature sig, object instance)
        {
            if (!sig.EventInterfaceType.IsAssignableFrom(instance.GetType()))
                throw new InvalidOperationException(
                    $"{instance.GetType().Name} does not implement {sig.EventInterfaceType.Name}.");

            var targetMethod = ResolveImplementationMethod(instance.GetType(), sig.EventInterfaceType, sig.Method);

            if (sig.Kind == EventSignatureKind.NoPayload)
                return Delegate.CreateDelegate(typeof(Action), instance, targetMethod);
            
            var delegateType = typeof(Action<>).MakeGenericType(sig.PayloadTypeOrNull);
            
            return Delegate.CreateDelegate(delegateType, instance, targetMethod);
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

            throw new MissingMethodException(
                $"Cannot find implementation for {interfaceType.Name}.{interfaceMethod.Name} on {concreteType.Name}.");
        }

        private void SendCacheToDelegate(EventSignature sig, Delegate del)
        {
            _cache.WithCache(sig.EventInterfaceType, cache =>
            {
                if (sig.Kind == EventSignatureKind.NoPayload)
                    (del as Action)?.Invoke();
                else
                    del.DynamicInvoke(cache.EventData);
            });
        }
    }
}
