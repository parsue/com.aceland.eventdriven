using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Disposable;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal : DisposableObject
    {
        private Signal(string id, Observers observers)
        {
            Id = id;
            _observers = observers;
        }

        ~Signal() => Dispose(false);

        protected override void DisposeManagedResources()
        {
            Signals.UnRegistrySignal(this);
            _observers.Dispose();
        }
    }
}