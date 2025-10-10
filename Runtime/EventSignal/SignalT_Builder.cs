using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Optional;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T>
    {
        public interface ISignalFinalBuilder<TValue>
        {
            ISignal<TValue> Build();
            IReadonlySignal<TValue> BuildReadonly();
        }

        internal class SignalBuilder<TValue> : ISignalFinalBuilder<TValue>
        {
            internal SignalBuilder(Option<string> id, TValue value)
            {
                _id = id;
                _value = value;
            }
            
            private Option<string> _id;
            private readonly TValue _value;

            public ISignal<TValue> Build() =>
                BuildSignal();

            public IReadonlySignal<TValue> BuildReadonly() =>
                SignalExtension.AsReadonly<TValue>(BuildSignal());

            private ISignal<TValue> BuildSignal()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var observers = new Observers<TValue>();
                var signal = new Signal<TValue>(id, observers, _value, false);
                Signals.RegistrySignal(signal);
                return signal;
            }
        }
    }
}
