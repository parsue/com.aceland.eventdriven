using System;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public interface ISignal : IEventSignal
    {
        void Dispose();
        bool Disposed { get; }
        
        void AddListener(Action listener, bool runImmediately = false);
        void RemoveListener(Action listener);
        void RemoveAllListeners();
        
        void Trigger();
    }

    public interface ISignal<T> : IEventSignal
    {
        T Value { get; set; }
        
        void Dispose();
        bool Disposed { get; }
        
        void AddListener(Action<T> listener, bool runImmediately = false);
        void RemoveListener(Action<T> listener);
        void RemoveAllListeners();
        
        void Trigger();
    }
}