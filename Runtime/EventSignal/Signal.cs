using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.PlayerLoopHack;
using AceLand.TaskUtils;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal : ISignal
    {
        public string Id { get; }
        private readonly Observers _observers;
        private ISignal _refSignal;
        private readonly bool _triggerOncePerFrame;
        private readonly PlayerLoopState _triggerState;
        private bool _triggeredInThisFrame;

        public void AddListener(Action listener, bool runImmediately = false)
        {
            _observers.AddListener(listener);
            if (runImmediately) listener?.Invoke();
        }

        public void RemoveListener(Action listener)
        {
            _observers.RemoveListener(listener);
        }

        public void RemoveAllListeners()
        {
            _observers.Clear();
        }

        public override string ToString() => Id;

        public void Trigger()
        {
            if (!_triggerOncePerFrame )
            {
                _observers.Trigger();
                return;
            }
            
            if (_triggeredInThisFrame) return;
            _triggeredInThisFrame = true;
            Promise.Dispatcher.Run(_observers.Trigger, _triggerState);
            Promise.Dispatcher.Run(() => _triggeredInThisFrame = false, _triggerState.Previous());
        }
    }
}