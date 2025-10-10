using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Disposable;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T> : DisposableObject
    {
        private Signal(string id, Observers<T> observers, T value, bool forceReadonly)
        {
            Id = id;
            _observers = observers;
            _value = value;
            _forceReadonly = forceReadonly;
        }

        ~Signal() => Dispose(false);
        
        protected override void DisposeManagedResources()
        {
            Signals.UnRegistrySignal(this);
            _observers.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
