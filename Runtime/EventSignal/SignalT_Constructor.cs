using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Lifecycle;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T> : IDisposable
    {
        private Signal(string id, Observers<T> observers, T value, SignalTriggerMethod triggerMethod, PlayerLoopPoint triggerState)
        {
            Id = id;
            _observers = observers;
            _value = value;
            _triggerMethod = triggerMethod;
            _triggerState = triggerState;
        }

        ~Signal() => Dispose();
        
        public bool Disposed { get; private set; }
        
        public virtual void Dispose()
        {
            if (Disposed) return;
            
            Signals.UnRegistrySignal(this);
            _observers.Dispose();
            GC.SuppressFinalize(this);
            Disposed = true;
        }
    }
}
