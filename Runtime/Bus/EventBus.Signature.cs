using System;
using System.Collections.Generic;
using UnityEngine;

namespace AceLand.EventDriven.Bus
{
    public static partial class EventBus
    {
        private static readonly Dictionary<Type, EventSignature> signatures = new();
        
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

        private static EventSignature BuildSignature(Type eventInterface)
        {
            EnsureIsEventInterface(eventInterface);

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
                {
                    var payloadType = pars[1].ParameterType;
                    return new EventSignature(eventInterface, m, EventSignatureKind.SinglePayload, payloadType);
                }
                default:
                    Debug.LogError($"Event method {eventInterface}.{m.Name} must have 1 or 2 parameters.");
                    return null;
            }
        }
    }
}