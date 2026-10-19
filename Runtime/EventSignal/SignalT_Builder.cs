using System;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public static partial class Signal<T>
    {
        public interface ISignalFinalBuilder<TValue>
        {
            ISignal<TValue> Build();
        }

        internal class SignalBuilder<TValue> : ISignalFinalBuilder<TValue>
        {
            internal SignalBuilder(string id, TValue value)
            {
                _id = id;
                _value = value;
            }
            
            private readonly string _id;
            private readonly TValue _value;

            public ISignal<TValue> Build() =>
                BuildSignal();

            private ISignal<TValue> BuildSignal()
            {
                var id = _id ?? Guid.NewGuid().ToString();
                var observers = new Observers<TValue>();
                var signal = new SignalObject<TValue>(id, observers, _value);
                Signals.RegistrySignal(signal);
                return signal;
            }
        }
    }
}
