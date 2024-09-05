using AceLand.Library.Disposable;
using System;
using System.Collections.Generic;

namespace AceLand.EventDriven.EventSignal.Core
{
    public class Observers : DisposableObject, IObservers
    {
        ~Observers() => Dispose(false);

        protected override void DisposeUnmanagedResources()
        {
            Listeners = null;
        }

        private Observers(params Action[] listeners)
        {
            foreach (var listener in listeners)
                Listeners += listener;
        }

        internal static ObserversBuilder Builder() => new ObserversBuilder();
        internal class ObserversBuilder
        {
            internal ObserversBuilder() => _listeners = new List<Action>();
            
            private readonly List<Action> _listeners;

            public Observers Build() => new(_listeners.ToArray());

            public ObserversBuilder WithActions(params Action[] listeners)
            {
                _listeners.AddRange(listeners);
                return this;
            }

            public ObserversBuilder WithAction(Action listener)
            {
                _listeners.Add(listener);
                return this;
            }
        }
        
        private event Action Listeners;

        public void AddListener(Action listener) => Listeners += listener;
        public void RemoveListener(Action listener) => Listeners += listener;

        public void Trigger()
        {
            Listeners?.Invoke();
        }
    }
    
    public class Observers<T> : DisposableObject, IObservers<T>
    {
        ~Observers() => Dispose(false);

        protected override void DisposeUnmanagedResources()
        {
            Listeners = null;
        }
        
        private Observers(params Action<T>[] listeners)
        {
            foreach (var listener in listeners)
                Listeners += listener;
        }

        internal static ObserversBuilder Builder() => new ObserversBuilder();
        internal class ObserversBuilder
        {
            internal ObserversBuilder() => _listeners = new List<Action<T>>();
            
            private readonly List<Action<T>> _listeners;

            public Observers<T> Build() => new(_listeners.ToArray());

            public ObserversBuilder WithActions(params Action<T>[] listeners)
            {
                _listeners.AddRange(listeners);
                return this;
            }

            public ObserversBuilder WithAction(Action<T> listener)
            {
                _listeners.Add(listener);
                return this;
            }
        }
        
        private event Action<T> Listeners;

        public void AddListener(Action<T> listener) => Listeners += listener;
        public void RemoveListener(Action<T> listener) => Listeners += listener;
        public void RemoveAllListeners() => Listeners = null;

        public void Trigger(in T value)
        {
            Listeners?.Invoke(value);
        }
    }
}