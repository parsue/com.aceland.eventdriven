using System;
using System.Collections.Generic;
using AceLand.EventDriven.ProjectSetting;

namespace AceLand.EventDriven.EventSignal.Core
{
    internal static class Signals
    {
        private static EventDrivenSettings Settings => Events.Settings;
        private static readonly Dictionary<string, ISignal> SignalsById = new();
        
        internal static int TryGetSignal<T>(string id, out T signal)
            where T : ISignal
        {
            signal = default;
            
            if (!SignalsById.TryGetValue(id, out var value))
                return 1;

            if (value is not T signalAsT)
                return 2;

            signal = signalAsT;
            return 0;
        }

        internal static void RegistrySignal(ISignal signal)
        {
            var key = signal.Id;
            if (SignalsById.ContainsKey(key) && !Settings.SwapSignalOnSameId)
            {
                throw new Exception($"Registry Signal error: Signal [{key}] exists");
            }
            
            if (Settings.SwapSignalOnSameId)
            {
                SwapSignal(signal);
                return;
            }
            
            SignalsById[key] = signal;
        }
        
        internal static void UnRegistrySignal(ISignal signal)
        {
            var key = signal.Id;
            if (!SignalsById.ContainsKey(key)) return;
            SignalsById.Remove(key);
        }

        private static void SwapSignal(ISignal signal)
        {
            var key = signal.Id;
            SignalsById[key].Dispose();
            SignalsById[key] = signal;
        }
    }
}