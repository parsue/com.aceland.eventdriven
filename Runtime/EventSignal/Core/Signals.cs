using System;
using System.Collections.Generic;

namespace AceLand.EventDriven.EventSignal.Core
{
    internal static class Signals
    {
        private static readonly Dictionary<string, IEventSignal> signalsById = new();
        
        internal static int TryGetSignal<T>(string id, out T signal)
            where T : IEventSignal
        {
            signal = default;
            
            if (!signalsById.TryGetValue(id, out var value))
                return 1;

            if (value is not T signalAsT)
                return 2;

            signal = signalAsT;
            return 0;
        }

        internal static void RegistrySignal<T>(T signal)
            where T : IEventSignal
        {
            var key = signal.Id;
            if (!signalsById.TryAdd(key, signal))
                throw new Exception($"Registry Signal error: Signal [{key}] exists");
        }
        
        internal static void UnRegistrySignal<T>(T signal)
            where T : IEventSignal
        {
            var key = signal.Id;
            signalsById.Remove(key);
        }
    }
}