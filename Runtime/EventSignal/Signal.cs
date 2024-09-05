using System;
using System.Collections.Generic;
using AceLand.Library.Extensions;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.EventDriven.Handler;

namespace AceLand.EventDriven.EventSignal
{
    public class Signal : SignalUnit
    {
        private Signal(string id, Observers observers)
        {
            Id = id;
            Observers = observers;
        }

        public static SignalGetter<Signal> Get(string id) => new (id); 
        
        public interface ISignalBuilder
        {
            Signal Build();
            ISignalBuilder WithId(string id);
            ISignalBuilder WithListeners(params Action[] listeners);
            ISignalBuilder WithListener(Action listener);
        }
        
        public static ISignalBuilder Builder() => new SignalBuilder();
        private class SignalBuilder : ISignalBuilder
        {
            internal SignalBuilder() => _listeners = new List<Action>();

            private string _id = string.Empty;
            private readonly List<Action> _listeners;

            public Signal Build()
            {
                if (_id.IsNullOrEmptyOrWhiteSpace())
                    _id = Guid.NewGuid().ToString();

                var observers = Observers.Builder()
                    .WithActions(_listeners.ToArray())
                    .Build();
                var signal = new Signal(_id, observers);
                Signals.RegistrySignal(signal);
                return signal;
            }

            public ISignalBuilder WithId(string id)
            {
                _id = id;
                return this;
            }

            public ISignalBuilder WithListeners(params Action[] listeners)
            {
                _listeners.AddRange(listeners);
                return this;
            }

            public ISignalBuilder WithListener(Action listener)
            {
                _listeners.Add(listener);
                return this;
            }
        }

        public void AddListener(Action listener) => Observers.AddListener(listener);
        public void RemoveListener(Action listener) => Observers.RemoveListener(listener);
    }
    
    public class Signal<T> : ReadonlySignal<T>
    {
        private Signal(string id, Observers<T> observers, T value) : base(id, observers, value) { }

        public static SignalGetter<T> Get(string id) => new (id); 
        
        public interface ISignalBuilder<TSignal>
        {
            Signal<TSignal> Build();
            ISignalBuilder<TSignal> WithId(string id);
            ISignalBuilder<TSignal> WithListeners(params Action<TSignal>[] listeners);
            ISignalBuilder<TSignal> WithListener(Action<TSignal> listener);
            ISignalBuilder<TSignal> WithValue(TSignal value);
        }
        
        public static ISignalBuilder<T> Builder() => new SignalBuilder();
        private class SignalBuilder : ISignalBuilder<T>
        {
            internal SignalBuilder() => _listeners = new List<Action<T>>();

            private string _id = string.Empty;
            private readonly List<Action<T>> _listeners;
            private T _value;

            public Signal<T> Build()
            {
                if (_id.IsNullOrEmptyOrWhiteSpace())
                    _id = Guid.NewGuid().ToString();

                var observers = Observers<T>.Builder()
                    .WithActions(_listeners.ToArray())
                    .Build();
                var signal = new Signal<T>(_id, observers, _value);
                Signals.RegistrySignal(signal);
                return signal;
            }

            public ISignalBuilder<T> WithId(string id)
            {
                _id = id;
                return this;
            }

            public ISignalBuilder<T> WithListeners(params Action<T>[] listeners)
            {
                _listeners.AddRange(listeners);
                return this;
            }

            public ISignalBuilder<T> WithListener(Action<T> listener)
            {
                _listeners.Add(listener);
                return this;
            }

            public ISignalBuilder<T> WithValue(T value)
            {
                _value = value;
                return this;
            }
        }

        public override T Value
        {
            get => SingleValue;
            set
            {
                base.SingleValue = value;
                Trigger();
            }
        }
        
        public void AddListener(Action<T> listener) => Observers.AddListener(listener);
        public void RemoveListener(Action<T> listener) => Observers.RemoveListener(listener);
        public void RemoveAllListeners() => Observers.RemoveAllListeners();

        public virtual void Trigger()
        {
            Observers.Trigger(Value);
        }
    }
}