using System;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    internal sealed partial class SignalObject<T> : ISignal<T>
    {
        public string Id { get; }

        private readonly Observers<T> _observers;
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                Trigger();
            }
        }

        public void AddListener(Action<T> listener, bool kickStart = false)
        {
            _observers.AddListener(listener);
            if (kickStart) listener?.Invoke(_value);
        }

        public void RemoveListener(Action<T> listener)
        {
            _observers.RemoveListener(listener);
        }

        public void RemoveAllListeners()
        {
            _observers.Clear();
        }

        public void Trigger()
        {
            _observers.Trigger(_value);
        }

        public override string ToString() => Value.ToString();

        public static implicit operator T(SignalObject<T> signal) => signal.Value;
    }
}
