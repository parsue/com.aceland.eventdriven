using System;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public interface ISignalLinker : IEventSignal
    {
        void Dispose();

        void AddAdaptor<T>(ISignalListener<T> signalListener, Predicate<T> condition, AdaptorOption option);
        void AddAdaptor<T>(ISignalListener<T> signalListener, T conditionValue, AdaptorOption option);
        
        void AddListener(Action<bool> listener, bool runImmediately = false);
        void RemoveListener(Action<bool> listener);
        void RemoveAllListeners();
        
        void Trigger();
    }
}