using System;
using System.Collections.Generic;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.EventDriven.Handler;
using AceLand.Library.Disposable;
using AceLand.Library.Optional;

namespace AceLand.EventDriven.EventSignal
{
    public class Signal<T> : DisposableObject, ISignal<T>
    {
        private Signal(string id, Observers<T> observers, T value)
        {
            Id = id;
            _observers = observers;
            _value = value;
        }

        #region Builder

        public static ISignalBuilder Builder() => new SignalBuilder();
        
        public interface ISignalBuilder
        {
            Signal<T> Build();
            ISignalBuilder WithId(string id);
            ISignalBuilder WithValue(T value);
            ISignalBuilder WithListener(Action<T> listener);
            ISignalBuilder WithListeners(params Action<T>[] listeners);
        }

        private class SignalBuilder : ISignalBuilder
        {
            private Option<string> _id = Option<string>.None();
            private readonly List<Action<T>> _listeners = new();
            private T _value;

            public Signal<T> Build()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var observers = new Observers<T>(_listeners.ToArray());
                var signal = new Signal<T>(id, observers, _value);
                Signals.RegistrySignal(signal);
                return signal;
            }

            public ISignalBuilder WithId(string id)
            {
                _id = id.ToOption();
                return this;
            }

            public ISignalBuilder WithValue(T value)
            {
                _value = value;
                return this;
            }

            public ISignalBuilder WithListener(Action<T> listener)
            {
                _listeners.Add(listener);
                return this;
            }

            public ISignalBuilder WithListeners(params Action<T>[] listeners)
            {
                _listeners.AddRange(listeners);
                return this;
            }
        }

        #endregion
        
        public static SignalGetter<T> Get(string id) => new (id); 
        public static ReadonlySignalGetter<T> GetReadonly(string id) => new (id); 
        
        public string Id { get; }
        private readonly Observers<T> _observers;
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                Trigger();
            }
        }
        
        protected override void DisposeManagedResources()
        {
            Signals.UnRegistrySignal(this);
            _observers.Dispose();
        }

        public void AddListener(Action<T> listener) =>
            _observers.AddListener(listener);

        public void RemoveListener(Action<T> listener) =>
            _observers.RemoveListener(listener);

        private void Trigger() => 
            _observers.Trigger(Value);
            
        public static implicit operator T(Signal<T> signal) => signal.Value;
    }
}
