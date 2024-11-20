using System;
using System.Collections.Generic;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Optional;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T>
    {
        public static ISignalBuilder Builder() => new SignalBuilder();
        
        public interface ISignalBuilder
        {
            Signal<T> Build();
            ISignalBuilder WithId(string id);
            ISignalBuilder WithId<TEnum>(TEnum id) where TEnum : Enum;
            ISignalBuilder WithValue(T value);
            ISignalBuilder WithListener(Action<T> listener);
            ISignalBuilder WithListeners(params Action<T>[] listeners);
            ISignalBuilder WithForceReadonly();
        }

        private class SignalBuilder : ISignalBuilder
        {
            private Option<string> _id = Option<string>.None();
            private readonly List<Action<T>> _listeners = new();
            private T _value;
            private bool _forceReadonly;

            public Signal<T> Build()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var observers = new Observers<T>(_listeners.ToArray());
                var signal = new Signal<T>(id, observers, _value, _forceReadonly);
                Signals.RegistrySignal(signal);
                return signal;
            }

            public ISignalBuilder WithId(string id)
            {
                _id = id.ToOption();
                return this;
            }

            public ISignalBuilder WithId<TEnum>(TEnum id) where TEnum : Enum
            {
                _id = id.ToString().ToOption();
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

            public ISignalBuilder WithForceReadonly()
            {
                _forceReadonly = true;
                return this;
            }
        }
    }
}
