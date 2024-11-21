using System;
using AceLand.Library.Disposable;

namespace AceLand.EventDriven.EventSignal.Core
{
    internal class Observers : DisposableObject, IObservers
    {
        internal Observers(params Action[] listeners)
        {
            foreach (var listener in listeners)
                Listeners += listener;
        }
        
        ~Observers() => Dispose(false);

        protected override void DisposeUnmanagedResources()
        {
            Listeners = null;
        }
        
        private event Action Listeners;

        public void AddListener(Action listener) => Listeners += listener;
        public void RemoveListener(Action listener) => Listeners -= listener;
        public void Clear() => Listeners = null;

        public void Trigger()
        {
            Listeners?.Invoke();
        }
    }
    
    internal class Observers<T> : DisposableObject, IObservers<T>
    {
        internal Observers(params Action<T>[] listeners)
        {
            foreach (var listener in listeners)
                Listeners += listener;
        }
        
        ~Observers() => Dispose(false);

        protected override void DisposeUnmanagedResources()
        {
            Listeners = null;
        }
        
        private event Action<T> Listeners;

        public void AddListener(Action<T> listener) => Listeners += listener;
        public void RemoveListener(Action<T> listener) => Listeners -= listener;
        public void Clear() => Listeners = null;

        public void Trigger(in T value)
        {
            Listeners?.Invoke(value);
        }
    }
}
