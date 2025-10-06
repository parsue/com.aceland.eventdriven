using System;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Optional;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal
    {
        public static ISignalBuilder Builder() => new SignalBuilder();
        
        public interface ISignalBuilder
        {
            ISignalFinalBuilder WithId(string id);
            ISignalFinalBuilder WithId<TEnum>(TEnum id) where TEnum : Enum;
        }

        public interface ISignalValueBuilder
        {
            Signal<T>.ISignalFinalBuilder<T> WithValue<T>(T value);
        }
        
        public interface ISignalFinalBuilder : ISignalValueBuilder
        {
            Signal Build();
        }
        
        private class SignalBuilder : ISignalBuilder, ISignalFinalBuilder
        {
            private Option<string> _id = Option<string>.None();

            public Signal Build()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var observers = new Observers();
                var signal = new Signal(id, observers);
                Signals.RegistrySignal(signal);
                return signal;
            }

            ISignalFinalBuilder ISignalBuilder.WithId(string id)
            {
                _id = id.ToOption();
                return this;
            }

            ISignalFinalBuilder ISignalBuilder.WithId<TEnum>(TEnum id)
            {
                _id = id.ToString().ToOption();
                return this;
            }

            public Signal<T>.ISignalFinalBuilder<T> WithValue<T>(T value)
            {
                return new Signal<T>.SignalBuilder<T>(_id, value);
            }
        }
    }
}