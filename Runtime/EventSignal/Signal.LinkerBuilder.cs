using System;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public static partial class Signal
    {
        public interface ISignalLinkerBuilder : ISignalLinkerFinalBuilder
        {
            ISignalLinkerFinalBuilder WithId(string id);
            ISignalLinkerFinalBuilder WithId<TEnum>(TEnum id) where TEnum : Enum;
        }

        public interface ISignalLinkerFinalBuilder
        {
            ISignalLinker Build();
        }

        private class SignalLinkerBuilder : ISignalLinkerBuilder
        {
            private string _id;
            
            public ISignalLinker Build()
            {
                var id = _id ?? Guid.NewGuid().ToString();
                var linker = SignalLinker.Create(id);
                Signals.RegistrySignal(linker);
                return linker;
            }

            public ISignalLinkerFinalBuilder WithId(string id)
            {
                _id = id;
                return this;
            }

            public ISignalLinkerFinalBuilder WithId<TEnum>(TEnum id) where TEnum : Enum
            {
                _id = id.ToString();
                return this;
            }
        }
        
    }
}
