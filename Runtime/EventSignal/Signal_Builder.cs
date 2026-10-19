using System;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public static partial class Signal
    {
        public static ISignalBuilder Builder() => new SignalBuilder();
        public static ISignalLinkerBuilder LinkerBuilder() => new SignalLinkerBuilder();
        
        public interface ISignalBuilder : ISignalFinalBuilder
        {
            ISignalFinalBuilder WithId(string id);
            ISignalFinalBuilder WithId<TEnum>(TEnum id) where TEnum : Enum;
        }

        public interface ISignalValueBuilder
        {
            Signal<T>.ISignalFinalBuilder<T> WithValue<T>(T value);
            Signal<T>.ISignalFinalBuilder<T> WithValue<T>();
        }
        
        public interface ISignalFinalBuilder : ISignalValueBuilder
        {
            ISignal Build();
        }
        
        private class SignalBuilder : ISignalBuilder
        {
            private string _id = null;

            public ISignal Build()
            {
                var id = _id ?? Guid.NewGuid().ToString();
                var observers = new Observers();
                var signal = new SignalObject(id, observers);
                Signals.RegistrySignal(signal);
                return signal;
            }

            public ISignalFinalBuilder WithId(string id)
            {
                _id = id;
                return this;
            }

            public ISignalFinalBuilder WithId<TEnum>(TEnum id) where TEnum : Enum
            {
                _id = id.ToString();
                return this;
            }

            public Signal<T>.ISignalFinalBuilder<T> WithValue<T>(T value)
            {
                return new Signal<T>.SignalBuilder<T>(_id, value);
            }

            public Signal<T>.ISignalFinalBuilder<T> WithValue<T>()
            {
                return new Signal<T>.SignalBuilder<T>(_id, default);
            }
        }
    }
}
