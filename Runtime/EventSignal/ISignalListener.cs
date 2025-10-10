using System;

namespace AceLand.EventDriven.EventSignal
{
    public interface ISignalListener : ISignalTrigger
    {
        void AddListener(Action listener, bool runImmediately = false);
        void RemoveListener(Action listener);
        void RemoveAllListeners();
    }
    
    public interface ISignalListener<out T> : ISignalTrigger
    {
        void AddListener(Action<T> listener, bool runImmediately = false);
        void RemoveListener(Action<T> listener);
        void RemoveAllListeners();
    }
}