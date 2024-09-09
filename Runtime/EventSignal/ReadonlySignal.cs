using System;
using AceLand.EventDriven.EventSignal.Core;

namespace AceLand.EventDriven.EventSignal
{
    public class ReadonlySignal<T> : IReadonlySignal<T>
    {
        internal ReadonlySignal(Signal<T> refSignal) =>
            _refSignal = refSignal;

        public string Id => _refSignal?.Id ?? null;
        public T Value => _refSignal.Value ?? default;

        private readonly Signal<T> _refSignal;

        public void AddListener(Action<T> listener) =>
            _refSignal.AddListener(listener);

        public void RemoveListener(Action<T> listener) =>
            _refSignal.RemoveListener(listener);
    }
}