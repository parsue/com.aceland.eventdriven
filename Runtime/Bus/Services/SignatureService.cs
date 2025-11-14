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
        private SignatureService() {}
        
        private readonly Dictionary<Type, EventSignature> _signatures = new();

        public void InitializeAndScan()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var iEvent = typeof(IEvent);

            var eventInterfaces = assemblies
                .AsValueEnumerable()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types.AsValueEnumerable().Where(t => t != null).ToArray(); }
                })
                .Where(t =>
                    t is { IsInterface: true } &&
                    iEvent.IsAssignableFrom(t) &&
                    t != iEvent
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
                Debug.LogError($"Event interface {eventInterface} must declare exactly one method.");
                return null;
            }

            var m = methods[0];
            if (m.ReturnType != typeof(void))
            {
                Debug.LogError($"Event method {eventInterface}.{m.Name} must return void.");
                return null;
            }

            var pars = m.GetParameters();
            switch (pars.Length)
            {
                case 1 when pars[0].ParameterType != typeof(object):
                    Debug.LogError($"First parameter of {eventInterface}.{m.Name} must be object sender.");
                    return null;
                case 1:
                    return new EventSignature(eventInterface, m, EventSignatureKind.NoPayload, null);
                case 2 when pars[0].ParameterType != typeof(object):
                    Debug.LogError($"First parameter of {eventInterface}.{m.Name} must be object sender.");
                    return null;
                case 2:
                    var payloadType = pars[1].ParameterType;
                    return new EventSignature(eventInterface, m, EventSignatureKind.SinglePayload, payloadType);
                default:
                    Debug.LogError($"Event method {eventInterface}.{m.Name} must have 1 or 2 parameters.");
                    return null;
            }
        }
    }
}