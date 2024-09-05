using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.EventDriven.Handler;

namespace AceLand.EventDriven.EventSignal
{
    public abstract class ReadonlySignal<T> : ReadonlySignalUnit<T>
    {
        private protected ReadonlySignal(string id, Observers<T> observers, T value)
        {
            Id = id;
            Observers = observers;
            SetValue(value);
        }

        public static ReadonlySignalGetter<T> GetReadonly(string id) => new (id); 

        public void AddListener(Action<T> listener) => Observers.AddListener(listener);
        public void RemoveListener(Action<T> listener) => Observers.RemoveListener(listener);
        public void RemoveAllListeners() => Observers.RemoveAllListeners();
    }
}