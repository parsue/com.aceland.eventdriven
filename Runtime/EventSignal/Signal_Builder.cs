using System;
using AceLand.EventDriven.Core;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Optional;
using AceLand.PlayerLoopHack;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal
    {
        public static ISignalBuilder Builder() => new SignalBuilder();
        
        public interface ISignalBuilder : ISignalTriggerStateBuilder
        {
            ISignalTriggerStateBuilder WithId(string id);
            ISignalTriggerStateBuilder WithId<TEnum>(TEnum id) where TEnum : Enum;
        }

        public interface ISignalTriggerStateBuilder : ISignalFinalBuilder
        {
            ISignalFinalBuilder WithTriggerOncePerFrame();
            ISignalFinalBuilder WithTriggerOncePerFrame(PlayerLoopState triggerState);
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
            private Option<string> _id = Option<string>.None();
            private SignalTriggerMethod _triggerMethod = SignalTriggerMethod.Immediately;
            private PlayerLoopState _triggerState;

            public ISignal Build()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var observers = new Observers();
                var signal = new Signal(id, observers, _triggerMethod, _triggerState);
                Signals.RegistrySignal(signal);
                return signal;
            }

            public ISignalTriggerStateBuilder WithId(string id)
            {
                _id = id.ToOption();
                return this;
            }

            public ISignalTriggerStateBuilder WithId<TEnum>(TEnum id) where TEnum : Enum
            {
                _id = id.ToString().ToOption();
                return this;
            }

            public ISignalFinalBuilder WithTriggerOncePerFrame()
            {
                _triggerMethod = SignalTriggerMethod.OncePerFrame;
                _triggerState = EventDrivenUtils.Settings.SignalTriggerState;
                return this;
            }

            public ISignalFinalBuilder WithTriggerOncePerFrame(PlayerLoopState triggerState)
            {
                _triggerMethod = SignalTriggerMethod.OncePerFrame;
                _triggerState = triggerState;
                return this;
            }

            public Signal<T>.ISignalFinalBuilder<T> WithValue<T>(T value)
            {
                return new Signal<T>.SignalBuilder<T>(_id, value, _triggerMethod, _triggerState);
            }

            public Signal<T>.ISignalFinalBuilder<T> WithValue<T>()
            {
                return new Signal<T>.SignalBuilder<T>(_id, default, _triggerMethod, _triggerState);
            }
        }
    }
}