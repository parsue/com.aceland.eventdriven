using System;

namespace AceLand.EventDriven.EventSignal.Core
{
    public interface IReadonlySignal<T>
    {
        string Id { get; }
        T Value { get; }
    }
}