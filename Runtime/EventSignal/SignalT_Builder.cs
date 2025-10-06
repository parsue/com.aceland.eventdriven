using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Optional;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T>
    {
        public interface ISignalFinalBuilder<TValue>
        {
            Signal<TValue> Build();
            Signal<TValue> BuildReadonly();
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

            public Signal<TValue> Build()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var observers = new Observers<TValue>();
                var signal = new Signal<TValue>(id, observers, _value, false);
                Signals.RegistrySignal(signal);
                return signal;
            }

            public Signal<TValue> BuildReadonly()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var observers = new Observers<TValue>();
                var signal = new Signal<TValue>(id, observers, _value, true);
                Signals.RegistrySignal(signal);
                return signal;
            }
        }
    }
}
