using System;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    internal sealed partial class SignalObject : ISignal
    {
        public string Id { get; }
        private readonly Observers _observers;
        private ISignal _refSignal;

        public void AddListener(Action listener, bool kickStart = false)
        {
            _observers.AddListener(listener);
            if (kickStart) listener?.Invoke();
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
            _observers.Trigger();
        }
    }
}
