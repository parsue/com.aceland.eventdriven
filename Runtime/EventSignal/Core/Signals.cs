using System;
using System.Collections.Generic;
using UnityEngine;

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
            {
                Debug.LogError($"Try Get Signal [{id}] Fail: type is not correct");
                return 2;
            }

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

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: dispose and clear all registered signals.
        /// In no-domain-reload play mode the static <see cref="signalsById"/> survives
        /// between play sessions, so it must be cleared on (re)initialization to avoid
        /// stale signals leaking across sessions. Player builds get a fresh static state
        /// per process launch, so this is not compiled there.
        /// </summary>
        internal static void ClearAll()
        {
            if (signalsById.Count == 0) return;

            // Snapshot and clear first so Dispose -> UnRegistrySignal becomes a no-op
            // and we avoid mutating the dictionary while iterating.
            var signals = new List<IEventSignal>(signalsById.Values);
            signalsById.Clear();

            foreach (var signal in signals)
            {
                if (signal is IDisposable disposable)
                    disposable.Dispose();
            }
        }
#endif
    }
}
