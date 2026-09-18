using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Lifecycle;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal : IDisposable
    {
        private Signal(string id, Observers observers, SignalTriggerMethod triggerMethod, PlayerLoopPoint triggerState)
        {
            Id = id;
            _observers = observers;
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