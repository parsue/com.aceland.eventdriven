using System;
using System.Reflection;

namespace AceLand.EventDriven.Bus.Core
{
    internal sealed class EventSignature
    {
        public Type EventInterfaceType { get; }
        public MethodInfo Method { get; }
        public EventSignatureKind Kind { get; }
        public Type PayloadTypeOrNull { get; }

        public EventSignature(Type eventInterfaceType, MethodInfo method, EventSignatureKind kind, Type payloadTypeOrNull)
        {
            EventInterfaceType = eventInterfaceType;
            Method = method;
            Kind = kind;
            PayloadTypeOrNull = payloadTypeOrNull;
        }
    }
}