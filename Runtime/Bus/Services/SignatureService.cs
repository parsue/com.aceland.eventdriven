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
            return _signatures.TryGetValue(eventInterface, out var sig)
                ? sig
                : throw new InvalidOperationException(
                    $"Event interface {eventInterface.Name} is not registered. " +
                    "Ensure the assembly declaring it references AceLand.EventDriven so the " +
                    "source generator can emit its registration, and that EventBus.Initialize() ran.");
        }

        public bool TryGetSignature(Type eventInterface, out EventSignature signature)
            => _signatures.TryGetValue(eventInterface, out signature);

        public IReadOnlyCollection<EventSignature> Signatures => _signatures.Values;
    }
}
