using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Lifecycle;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal : ISignal
    {
        public string Id { get; }
        private readonly Observers _observers;
        private ISignal _refSignal;
        private readonly SignalTriggerMethod _triggerMethod;
        private readonly PlayerLoopPoint _triggerState;
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
            switch (_triggerMethod)
            {
                case SignalTriggerMethod.Immediately:
                    _observers.Trigger();
                    return;
                
                case SignalTriggerMethod.OncePerFrame:
                    if (_triggeredInThisFrame) return;
                    _triggeredInThisFrame = true;
                    LifecycleFrame.RunNextFrame(SystemTrigger, _triggerState);
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SystemTrigger()
        {
            _observers.Trigger();
            _triggeredInThisFrame = false;
        }
    }
}