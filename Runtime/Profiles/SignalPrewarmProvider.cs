using System;
using AceLand.EventDriven.EventSignal;
using UnityEngine;
using Signal = AceLand.EventDriven.EventSignal.Signal;

namespace AceLand.EventDriven.Profiles
{
    public abstract class SignalPrewarmProvider<TId, TValue> : SignalProviderBase
        where TId : Enum
    {
        [Header("Signal Prewarm Provider")]
        [SerializeField] protected TId signalId;
        [SerializeField] protected TValue signalValue;

        public ISignal<TValue> GetSignal() => _signal;
        private ISignal<TValue> _signal;
        
        public override void PrewarmSignal()
        {
            _signal = Signal.Builder()
                .WithId(signalId)
                .WithValue(signalValue)
                .Build();
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

        public ISignal GetSignal() => _signal;
        private ISignal _signal;
        
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