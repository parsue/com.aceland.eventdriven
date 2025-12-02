using System;
using System.Collections.Generic;
using AceLand.EventDriven.Core;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Optional;
using AceLand.PlayerLoopHack;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal
    {
        public interface ISignalLinkerBuilder : ISignalLinkerTriggerStateBuilder
        {
            ISignalLinkerTriggerStateBuilder WithId(string id);
            ISignalLinkerTriggerStateBuilder WithId<TEnum>(TEnum id) where TEnum : Enum;
        }

        public interface ISignalLinkerTriggerStateBuilder : ISignalLinkerFinalBuilder
        {
            ISignalLinkerFinalBuilder WithTriggerOncePerFrame();
            ISignalLinkerFinalBuilder WithTriggerOncePerFrame(PlayerLoopState triggerState);
        }

        public interface ISignalLinkerFinalBuilder
        {
            ISignalLinker Build();
        }

        private class SignalLinkerBuilder : ISignalLinkerBuilder
        {
            private Option<string> _id = Option<string>.None();
            private SignalTriggerMethod _triggerMethod = SignalTriggerMethod.Immediately;
            private PlayerLoopState _triggerState;
            
            public ISignalLinker Build()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var linker = SignalLinker.Create(id, _triggerMethod, _triggerState);
                Signals.RegistrySignal(linker);
                return linker;
            }

            public ISignalLinkerTriggerStateBuilder WithId(string id)
            {
                _id = id.ToOption();
                return this;
            }

            public ISignalLinkerTriggerStateBuilder WithId<TEnum>(TEnum id) where TEnum : Enum
            {
                _id = id.ToString().ToOption();
                return this;
            }

            public ISignalLinkerFinalBuilder WithTriggerOncePerFrame()
            {
                _triggerMethod = SignalTriggerMethod.OncePerFrame;
                _triggerState = EventDrivenUtils.Settings.SignalTriggerState;
                return this;
            }

            public ISignalLinkerFinalBuilder WithTriggerOncePerFrame(PlayerLoopState triggerState)
            {
                _triggerMethod = SignalTriggerMethod.OncePerFrame;
                _triggerState = triggerState;
                return this;
            }
        }
        
    }
}