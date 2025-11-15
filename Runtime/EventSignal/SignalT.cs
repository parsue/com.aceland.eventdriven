using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.PlayerLoopHack;
using AceLand.TaskUtils;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T> : ISignal<T>
    {
        public string Id { get; }
        private readonly Observers<T> _observers;
        private T _value;
        private readonly bool _triggerOncePerFrame;
        private readonly PlayerLoopState _triggerState;
        private bool _triggeredInThisFrame;
        
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                Trigger();
            }
        }

        public void AddListener(Action<T> listener, bool runImmediately = false)
        {
            _observers.AddListener(listener);
            if (runImmediately) listener?.Invoke(_value);
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
            if (!_triggerOncePerFrame )
            {
                _observers.Trigger(_value);
                return;
            }
            
            if (_triggeredInThisFrame) return;
            _triggeredInThisFrame = true;
            Promise.Dispatcher.Run(() => _observers.Trigger(_value), _triggerState);
            Promise.Dispatcher.Run(() => _triggeredInThisFrame = false, _triggerState.Previous());
        }

        public override string ToString() => Value.ToString();
            
        public static implicit operator T(Signal<T> signal) => signal.Value;
    }
}
