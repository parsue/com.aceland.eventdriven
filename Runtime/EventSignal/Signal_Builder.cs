using System;
using System.Collections.Generic;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Optional;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal
    {
        public static ISignalBuilder Builder() => new SignalBuilder();
        
        public interface ISignalBuilder
        {
            Signal Build();
            ISignalBuilder WithId(string id);
            ISignalBuilder WithId<TEnum>(TEnum id) where TEnum : Enum;
            ISignalBuilder WithListener(Action listener);
            ISignalBuilder WithListeners(params Action[] listeners);
        }

        private class SignalBuilder : ISignalBuilder
        {
            private Option<string> _id = Option<string>.None();
            private readonly List<Action> _listeners = new();

            public Signal Build()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var observers = new Observers(_listeners.ToArray());
                var signal = new Signal(id, observers);
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

            public ISignalBuilder WithListener(Action listener)
            {
                _listeners.Add(listener);
                return this;
            }

            public ISignalBuilder WithListeners(params Action[] listeners)
            {
                _listeners.AddRange(listeners);
                return this;
            }
        }
    }
}