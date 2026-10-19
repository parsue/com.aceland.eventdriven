using System;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    internal sealed partial class SignalObject : IDisposable
    {
        internal SignalObject(string id, Observers observers)
        {
            Id = id;
            _observers = observers;
        }

        ~SignalObject() => Dispose();
        
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            if (Disposed) return;

            Signals.UnRegistrySignal(this);
            _observers.Dispose();
            GC.SuppressFinalize(this);
            Disposed = true;
        }
    }
}
