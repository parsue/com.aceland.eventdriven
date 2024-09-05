using System;
using System.Collections.Generic;
using AceLand.EventDriven.ProjectSetting;
using UnityEngine;

namespace AceLand.EventDriven.EventSignal.Core
{
    public static class Signals
    {
        private static EventDrivenSettings Settings => EventDrivenHelper.Settings;
        private static readonly Dictionary<string, ISignal> _signalsById = new();
        
        internal static int TryGetSignal<T>(string id, out T signal)
            where T : ISignal
        {
            signal = default;
            
            if (!_signalsById.TryGetValue(id, out var value))
                return 1;

            if (value is not T signalAsT)
                return 2;

            signal = signalAsT;
            return 0;
        }

        internal static void RegistrySignal(ISignal signal)
        {
            var key = signal.Id;
            if (_signalsById.ContainsKey(key) && !Settings.SwapSignalOnSameId)
            {
                throw new Exception($"Registry Signal error: Signal [{key}] exists");
            }
            
            if (Settings.SwapSignalOnSameId)
            {
                SwapSignal(signal);
                return;
            }
            
            _signalsById[key] = signal;
        }
        
        internal static void UnRegistrySignal(ISignal signal)
        {
            var key = signal.Id;
            if (!_signalsById.ContainsKey(key)) return;
            _signalsById.Remove(key);
        }

        private static void SwapSignal(ISignal signal)
        {
            var key = signal.Id;
            _signalsById[key].Dispose();
            _signalsById[key] = signal;
        }
    }
}