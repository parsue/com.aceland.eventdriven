using System;
using AceLand.EventDriven.Core;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Disposable;
using AceLand.PlayerLoopHack;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal : DisposableObject
    {
        private Signal(string id, Observers observers, bool triggerOncePerFrame, PlayerLoopState triggerState)
        {
            Id = id;
            _observers = observers;
            _triggerOncePerFrame = triggerOncePerFrame;
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