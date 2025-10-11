using System;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public interface ISignalListener : IEventSignal, IEventSignalRef
    {
        void AddListener(Action listener, bool runImmediately = false);
        void RemoveListener(Action listener);
        void RemoveAllListeners();
    }
    
    public interface ISignalListener<T> : IEventSignal, IEventSignalRef<T>
    {
        void AddListener(Action<T> listener, bool runImmediately = false);
        void RemoveListener(Action<T> listener);
        void RemoveAllListeners();
    }
}