﻿using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public interface IReadonlySignal<T> : ISignalListener<T>
    {
        T Value { get; }
        string ToString();
    }
}