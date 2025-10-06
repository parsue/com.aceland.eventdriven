using System;
using AceLand.EventDriven.EventSignal;
using UnityEngine;

namespace AceLand.EventDriven.Profiles
{
    public abstract class SignalPrewarmProvider<TId, TValue> : SignalProviderBase
        where TId : Enum
    {
        [Header("Signal Prewarm Provider")]
        [SerializeField] protected TId signalId;
        [SerializeField] protected TValue signalValue;

        public Signal<TValue> GetSignal() => _signal;
        private Signal<TValue> _signal;
        
        public override void PrewarmSignal()
        {
            var idBuilder = Signal.Builder()
                .WithId(signalId)
                .WithValue(signalValue);
            
            idBuilder.Build();
        }

        public override void Dispose()
        {
            _signal.Dispose();
        }
    }
    
    public abstract class SignalPrewarmProvider<TId> : SignalProviderBase
        where TId : Enum
    {
        [Header("Signal Prewarm Provider")]
        [SerializeField] protected TId signalId;

        public Signal GetSignal() => _signal;
        private Signal _signal;
        
        public override void PrewarmSignal()
        {
            _signal = Signal.Builder()
                .WithId(signalId)
                .Build();
        }

        public override void Dispose()
        {
            _signal.Dispose();
        }
    }
}