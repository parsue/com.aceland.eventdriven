using System;

namespace AceLand.EventDriven.EventSignal.Core
{
    public interface ISignal
    {
        string Id { get; }
        void Dispose();
    }

    public interface ISignal<T> : ISignal
    {
        T Value { get; set; }
    }
}