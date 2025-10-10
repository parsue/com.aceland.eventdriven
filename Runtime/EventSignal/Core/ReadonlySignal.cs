using System;

namespace AceLand.EventDriven.EventSignal.Core
{
    public class ReadonlySignal<T> : IReadonlySignal<T>
    {
        internal ReadonlySignal(ISignal<T> refSignal) =>
            _refSignal = refSignal;

        public string Id => _refSignal.Id;
        public T Value => _refSignal.Value ?? default;

        private readonly ISignal<T> _refSignal;

        public void AddListener(Action<T> listener, bool runImmediately = false) =>
            _refSignal.AddListener(listener, runImmediately);

        public void RemoveListener(Action<T> listener) =>
            _refSignal.RemoveListener(listener);

        public void RemoveAllListeners() =>
            _refSignal.RemoveAllListeners();

        public void Trigger() => _refSignal.Trigger();

        public override string ToString() => Value.ToString();
    }
}
