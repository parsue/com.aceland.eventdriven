using System;
using System.Collections.Generic;
using System.Linq;

namespace AceLand.EventDriven.EventSignal.Core
{
    internal class SignalLinker : IDisposable, ISignalLinker
    {
        internal static SignalLinker Create(string id) => new(id);
        
        private SignalLinker(string id)
        {
            Id = id;
        }
        
        ~SignalLinker() => Dispose();
        
        public virtual void Dispose()
        {
            Signals.UnRegistrySignal(this);
            _observers.Dispose();
            GC.SuppressFinalize(this);
        }
        
        public string Id { get; }
        
        private readonly Dictionary<AdaptorOption, ISignalAdaptor> adaptors = new();
        private readonly Observers<bool> _observers = new();
        private bool? _lastResult;

        public void AddAdaptor<T>(
            ISignalListener<T> signalListener,
            Predicate<T> condition,
            AdaptorOption option
        )
        {
            if (!adaptors.TryGetValue(option, out var adaptor))
            {
                adaptor = SignalAdaptor.Create<T>(option);
                adaptors[option] = adaptor;
            }
            adaptor.AddAdaptor(signalListener, condition, Trigger);
        }

        public void AddAdaptor<T>(
            ISignalListener<T> signalListener,
            T conditionValue,
            AdaptorOption option
        )
        {
            if (!adaptors.TryGetValue(option, out var adaptor))
            {
                adaptor = SignalAdaptor.Create<T>(option);
                adaptors[option] = adaptor;
            }
            adaptor.AddAdaptor(signalListener, v => EqualityComparer<T>.Default.Equals(v, conditionValue) , Trigger);
        }

        public void AddListener(Action<bool> listener, bool kickStart = false)
        {
            _observers.AddListener(listener);
            if (kickStart) listener?.Invoke(GetResult());
        }

        public void RemoveListener(Action<bool> listener)
        {
            _observers.RemoveListener(listener);
        }

        public void RemoveAllListeners()
        {
            _observers.Clear();
        }

        public void Trigger()
        {
            var result = GetResult();

            // Only notify observers on the first evaluation or when the computed
            // result actually changes. Adaptor value changes that do not flip the
            // linker's aggregated result should not re-trigger listeners.
            if (_lastResult == result) return;

            _lastResult = result;
            _observers.Trigger(result);
        }

        private bool GetResult()
        {
            return adaptors.Values.All(a => a.Result());
        }
    }
}
