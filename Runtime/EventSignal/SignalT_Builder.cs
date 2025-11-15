using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Optional;
using AceLand.PlayerLoopHack;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T>
    {
        public interface ISignalFinalBuilder<TValue>
        {
            ISignal<TValue> Build();
        }

        internal class SignalBuilder<TValue> : ISignalFinalBuilder<TValue>
        {
            internal SignalBuilder(Option<string> id, TValue value, bool triggerOncePerFrame, PlayerLoopState triggerState)
            {
                _id = id;
                _value = value;
                _triggerOncePerFrame = triggerOncePerFrame;
                _triggerState = triggerState;
            }
            
            private Option<string> _id;
            private readonly TValue _value;
            private readonly bool _triggerOncePerFrame;
            private readonly PlayerLoopState _triggerState;

            public ISignal<TValue> Build() =>
                BuildSignal();

            private ISignal<TValue> BuildSignal()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var observers = new Observers<TValue>();
                var signal = new Signal<TValue>(id, observers, _value, _triggerOncePerFrame, _triggerState);
                Signals.RegistrySignal(signal);
                return signal;
            }
        }
    }
}
