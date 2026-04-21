using System;
using System.Collections.Generic;
using System.Reflection;
using AceLand.EventDriven.Bus.Core;
using UnityEngine;
using ZLinq;

namespace AceLand.EventDriven.Bus.Services
{
    internal sealed class SignatureService
    {
        public static SignatureService Build() => new();
        private SignatureService()
        {
            _signatures = new();
        }

        private readonly Dictionary<Type, EventSignature> _signatures = new();

        public void InitializeAndScan()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var iBusEvent = typeof(IBusEvent);

            var eventInterfaces = assemblies
                .AsValueEnumerable()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types.AsValueEnumerable().Where(t => t != null).ToArray(); }
                })
                .Where(t =>
                    t is { IsInterface: true } &&
                    iBusEvent.IsAssignableFrom(t) &&
                    t != iBusEvent &&
                    t != typeof(IEvent) &&
                    (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(IEvent<>))
                )
                .ToArray();

            BootstrapRegister(eventInterfaces);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var listenerTypes = assemblies
                .AsValueEnumerable()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types.AsValueEnumerable().Where(t => t != null).ToArray(); }
                })
                .Where(t =>
                    t is { IsClass: true, IsAbstract: false } &&
                    t.GetInterfaces().AsValueEnumerable().Any(i => eventInterfaces.AsValueEnumerable().Contains(i))
                )
                .ToArray();

            Debug.Log($"EventBus: Registered {eventInterfaces.Length} event interfaces. Potential listener types: {listenerTypes.Length}.");
#endif
        }

        private void BootstrapRegister(IEnumerable<Type> eventInterfaces)
        {
            _signatures.Clear();

            foreach (var it in eventInterfaces)
            {
                var sig = BuildSignature(it);
                if (sig == null) continue;
                _signatures[it] = sig;
            }
        }

        public EventSignature GetOrThrowSignature(Type eventInterface)
        {
            return _signatures.TryGetValue(eventInterface, out var sig)
                ? sig
                : throw new InvalidOperationException(
                    $"Event interface {eventInterface.Name} is not registered. Ensure EventBus.Initialize() ran.");
        }

        private static EventSignature BuildSignature(Type eventInterface)
        {
            EventBus.EnsureIsEventInterface(eventInterface);

            var methods = eventInterface.GetMethods();
            if (methods.Length != 1)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"Event interface {eventInterface} must declare exactly one method.");
#endif
                return null;
            }

            var m = methods[0];
            if (m.ReturnType != typeof(void))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"Event method {eventInterface}.{m.Name} must return void.");
#endif
                return null;
            }

            var pars = m.GetParameters();
            var isNoData = typeof(IEvent).IsAssignableFrom(eventInterface);
            var isWithData = eventInterface.GetInterfaces().AsValueEnumerable().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEvent<>));

            if (isNoData && isWithData)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"Event interface {eventInterface} cannot implement both IEvent and IEvent<T>.");
#endif
                return null;
            }

            if (!isNoData && !isWithData)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"Event interface {eventInterface} must implement either IEvent or IEvent<T>.");
#endif
                return null;
            }

            switch (pars.Length)
            {
                case 1 when pars[0].ParameterType != typeof(object):
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError($"First parameter of {eventInterface}.{m.Name} must be object sender.");
#endif
                    return null;
                case 1:
                    if (!isNoData)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogError($"Event method {eventInterface}.{m.Name} has 1 parameter but does not implement IEvent.");
#endif
                        return null;
                    }
                    return new EventSignature(eventInterface, m, EventSignatureKind.NoPayload, null);
                case 2 when pars[0].ParameterType != typeof(object):
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError($"First parameter of {eventInterface}.{m.Name} must be object sender.");
#endif
                    return null;
                case 2:
                    if (!isWithData)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogError($"Event method {eventInterface}.{m.Name} has 2 parameters but does not implement IEvent<T>.");
#endif
                        return null;
                    }
                    
                    var payloadType = pars[1].ParameterType;
                    var withDataInterface = eventInterface.GetInterfaces().AsValueEnumerable().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEvent<>));
                    var genericArg = withDataInterface.GetGenericArguments()[0];
                    
                    // --- VALIDATION: Check if TData matches the method's payload parameter ---
                    if (payloadType != genericArg)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogError($"[EventBus] Type Mismatch in {eventInterface.Name}! It implements IEvent<{genericArg.Name}>, but the method '{m.Name}' expects a payload of type '{payloadType.Name}'. These must be exactly the same.");
#endif
                        return null;
                    }

                    return new EventSignature(eventInterface, m, EventSignatureKind.SinglePayload, payloadType);
                default:
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError($"Event method {eventInterface}.{m.Name} must have 1 or 2 parameters.");
#endif
                    return null;
            }
        }
    }
}
