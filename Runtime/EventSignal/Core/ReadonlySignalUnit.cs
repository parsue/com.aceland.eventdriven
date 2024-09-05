using AceLand.Library.Disposable;
using System;

namespace AceLand.EventDriven.EventSignal.Core
{
    public abstract class ReadonlySignalUnit<T> : DisposableObject, ISignal
    {
        ~ReadonlySignalUnit() => Dispose(false);

        protected override void BeforeDispose()
        {
            Signals.UnRegistrySignal(this);
            Observers.Dispose();
        }

        public string Id { get; private protected set; }
        protected Observers<T> Observers { get; set; }
        protected T SingleValue;

        public virtual T Value
        {
            get => SingleValue;
            set => throw new Exception("Cannot change value in Readonly Signal");
        }

        internal void SetValue(T value) => this.SingleValue = value;
    }
}