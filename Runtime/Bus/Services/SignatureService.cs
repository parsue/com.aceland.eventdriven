using System;
using System.Collections.Generic;
using AceLand.EventDriven.Bus.Core;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace AceLand.EventDriven.Bus.Services
{
    internal sealed class SignatureService
    {
        public static SignatureService Build() => new();

        private SignatureService()
        {
            _signatures = new Dictionary<Type, EventSignature>();
        }

        private readonly Dictionary<Type, EventSignature> _signatures;

        /// <summary>
        /// Snapshot the compile-time signatures pushed by the source-generated
        /// per-assembly bootstrap. No reflection / assembly scanning.
        /// </summary>
        public void InitializeFromGenerated()
        {
            _signatures.Clear();

            foreach (var kvp in EventBus.GeneratedSignatureStore)
                _signatures[kvp.Key] = kvp.Value;

#if UNITY_EDITOR
            Debug.Log($"[EventBus] Registered {_signatures.Count} event interfaces (source-generated).");
#endif
        }

        public EventSignature GetOrThrowSignature(Type eventInterface)
        {
            return TryGetSignature(eventInterface, out var sig)
                ? sig
                : throw new InvalidOperationException(
                    $"Event interface {eventInterface.Name} is not registered. " +
                    "Ensure the assembly declaring it references AceLand.EventDriven so the " +
                    "source generator can emit its registration, and that EventBus.Initialize() ran.");
        }

        public bool TryGetSignature(Type eventInterface, out EventSignature signature)
        {
            if (_signatures.TryGetValue(eventInterface, out signature))
                return true;

            // Fallback: the one-time snapshot taken in InitializeFromGenerated() can miss
            // signatures whose generated per-assembly bootstrap ran AFTER EventBus.Initialize().
            // Unity does not guarantee ordering between [RuntimeInitializeOnLoadMethod] entries
            // that share the same load type, so re-check the live process-lifetime store and
            // promote any late arrival into the local snapshot.
            if (EventBus.TryGetGeneratedSignature(eventInterface, out signature))
            {
                _signatures[eventInterface] = signature;
                return true;
            }

            signature = default;
            return false;
        }

        public IReadOnlyCollection<EventSignature> Signatures => _signatures.Values;
    }
}
