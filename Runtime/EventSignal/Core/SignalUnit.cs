using AceLand.Library.Disposable;

namespace AceLand.EventDriven.EventSignal.Core
{
    public abstract class SignalUnit : DisposableObject, ISignal
    {
        ~SignalUnit() => Dispose(false);

        protected override void BeforeDispose()
        {
            Signals.UnRegistrySignal(this);
            Observers.Dispose();
        }

        public string Id { get; private protected set; }
        protected Observers Observers { get; set; }

        public virtual void Trigger()
        {
            Observers.Trigger();
        }
    }
}