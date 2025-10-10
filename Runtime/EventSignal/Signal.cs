using System;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal : ISignal
    {
        public string Id { get; }
        private readonly Observers _observers;

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

        public void Trigger() => 
            _observers.Trigger();
    }
}