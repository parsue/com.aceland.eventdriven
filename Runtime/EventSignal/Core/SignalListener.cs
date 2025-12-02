using System;

namespace AceLand.EventDriven.EventSignal.Core
{
    internal class SignalListener : ISignalListener
    {
        internal static SignalListener Build(ISignal refSignal) =>
            new(refSignal);
        
        private SignalListener(ISignal refSignal) =>
            _refSignal = refSignal;

        ISignal IEventSignalRef.RefSignal => _refSignal;
        private readonly ISignal _refSignal;

        public string Id => _refSignal.Id;

        public bool Disposed => _refSignal.Disposed;

        public void AddListener(Action listener, bool runImmediately = false) =>
            _refSignal.AddListener(listener, runImmediately);

        public void RemoveListener(Action listener) =>
            _refSignal.RemoveListener(listener);

        public void RemoveAllListeners() =>
            _refSignal.RemoveAllListeners();
    }
    
    internal class SignalListener<T> : ISignalListener<T>
    {
        internal static SignalListener<T> Build(ISignal<T> refSignal) =>
            new(refSignal);
        
        private SignalListener(ISignal<T> refSignal) =>
            _refSignal = refSignal;


        ISignal<T> IEventSignalRef<T>.RefSignal => _refSignal;
        private readonly ISignal<T> _refSignal;

        public string Id => _refSignal.Id;

        public bool Disposed => _refSignal.Disposed;
        public T Value => _refSignal.Value ?? default;

        public void AddListener(Action<T> listener, bool runImmediately = false) =>
            _refSignal.AddListener(listener, runImmediately);

        public void RemoveListener(Action<T> listener) =>
            _refSignal.RemoveListener(listener);

        public void RemoveAllListeners() =>
            _refSignal.RemoveAllListeners();
    }
}