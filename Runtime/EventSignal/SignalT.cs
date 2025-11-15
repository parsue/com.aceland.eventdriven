using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.PlayerLoopHack;
using AceLand.TaskUtils;
using UnityEngine;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T> : ISignal<T>
    {
        public string Id { get; }

        private readonly Observers<T> _observers;
        private T _value;
        private readonly SignalTriggerMethod _triggerMethod;
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
            switch (_triggerMethod)
            {
                case SignalTriggerMethod.Immediately:
                    _observers.Trigger(_value);
                    return;
                
                case SignalTriggerMethod.OncePerFrame:
                    if (_triggeredInThisFrame) return;
                    _triggeredInThisFrame = true;
                    Promise.Dispatcher.Run(SystemTrigger, _triggerState);
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SystemTrigger()
        {
            _observers.Trigger(_value);
            _triggeredInThisFrame = false;
        }

        public override string ToString() => Value.ToString();
            
        public static implicit operator T(Signal<T> signal) => signal.Value;
    }
}
