using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Disposable;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T> : DisposableObject
    {
        private Signal(string id, Observers<T> observers, T value, bool readonlyToObserver)
        {
            Id = id;
            _observers = observers;
            _value = value;
            _readonlyToObserver = readonlyToObserver;
        }

        ~Signal() => Dispose(false);
        
        protected override void DisposeManagedResources()
        {
            Signals.UnRegistrySignal(this);
            _observers.Dispose();
        }
    }
}
