using System;
using AceLand.Disposable;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.PlayerLoopHack;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal : DisposableObject
    {
        private Signal(string id, Observers observers, SignalTriggerMethod triggerMethod, PlayerLoopState triggerState)
        {
            Id = id;
            _observers = observers;
            _triggerMethod = triggerMethod;
            _triggerState = triggerState;
        }

        ~Signal() => Dispose(false);

        protected override void DisposeManagedResources()
        {
            Signals.UnRegistrySignal(this);
            _observers.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}