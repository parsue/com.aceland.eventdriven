using System;

namespace AceLand.EventDriven.EventSignal.Core
{
    public class ReadonlySignal<T> : IReadonlySignal<T>
    {
        internal static ReadonlySignal<T> Build(ISignal<T> refSignal) =>
            new(refSignal);
        
        private ReadonlySignal(ISignal<T> refSignal) =>
            _refSignal = refSignal;

        ISignal<T> IEventSignalRef<T>.RefSignal => _refSignal;
        private readonly ISignal<T> _refSignal;

        public string Id => _refSignal.Id;

        public T Value => _refSignal.Value ?? default;

        public void AddListener(Action<T> listener, bool runImmediately = false) =>
            _refSignal.AddListener(listener, runImmediately);

        public void RemoveListener(Action<T> listener) =>
            _refSignal.RemoveListener(listener);

        public void RemoveAllListeners() =>
            _refSignal.RemoveAllListeners();
    }
}
