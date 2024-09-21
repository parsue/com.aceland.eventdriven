using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Disposable;
using AceLand.Library.Optional;
using AceLand.TaskUtils;
using AceLand.TaskUtils.PromiseAwaiter;

namespace AceLand.EventDriven.EventSignal
{
    public class Signal<T> : DisposableObject, ISignal<T>,
        IComparable<Signal<T>>, IComparable<ReadonlySignal<T>>, IComparable<T>,
        IEquatable<Signal<T>>, IEquatable<ReadonlySignal<T>>, IEquatable<T>
    {
        private Signal(string id, Observers<T> observers, T value, bool readonlyToObserver)
        {
            Id = id;
            _observers = observers;
            _value = value;
            _readonlyToObserver = readonlyToObserver;
        }

        ~Signal() => Dispose(false);

        #region Builder

        public static ISignalBuilder Builder() => new SignalBuilder();
        
        public interface ISignalBuilder
        {
            Signal<T> Build();
            ISignalBuilder WithId(string id);
            ISignalBuilder WithValue(T value);
            ISignalBuilder WithListener(Action<T> listener);
            ISignalBuilder WithListeners(params Action<T>[] listeners);
            ISignalBuilder ReadonlyToObserver(bool readonlyToObserver);
        }

        private class SignalBuilder : ISignalBuilder
        {
            private Option<string> _id = Option<string>.None();
            private readonly List<Action<T>> _listeners = new();
            private T _value;
            private bool _readonlyToObserver;

            public Signal<T> Build()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var observers = new Observers<T>(_listeners.ToArray());
                var signal = new Signal<T>(id, observers, _value, _readonlyToObserver);
                Signals.RegistrySignal(signal);
                return signal;
            }

            public ISignalBuilder WithId(string id)
            {
                _id = id.ToOption();
                return this;
            }

            public ISignalBuilder WithValue(T value)
            {
                _value = value;
                return this;
            }

            public ISignalBuilder WithListener(Action<T> listener)
            {
                _listeners.Add(listener);
                return this;
            }

            public ISignalBuilder WithListeners(params Action<T>[] listeners)
            {
                _listeners.AddRange(listeners);
                return this;
            }

            public ISignalBuilder ReadonlyToObserver(bool readonlyToObserver)
            {
                _readonlyToObserver = readonlyToObserver;
                return this;
            }
        }

        #endregion

        #region Getter

        public static Promise<Signal<T>> Get(string id) => GetSignal(id); 
        public static Promise<ReadonlySignal<T>> GetReadonly(string id) => GetReadonlySignal(id);

        private static async Task<Signal<T>> GetSignal(string id)
        {
            var startTime = DateTime.Now;
            var timeout = EventDrivenHelper.Settings.SignalGetterTimeout;
            var aliveToken = TaskHelper.ApplicationAliveToken;
            string msg;
            
            while (!aliveToken.IsCancellationRequested && (DateTime.Now - startTime).TotalSeconds < timeout)
            {
                await Task.Yield();
                
                var arg = Signals.TryGetSignal(id, out Signal<T> signal);
                switch (arg)
                {
                    case 0:
                        return signal;
                    case 2:
                        msg = $"Get Signal [{id}] fail: wrong type";
                        throw new Exception(msg);
                }
            }

            msg = $"Signal [{id}] is not found";
            throw new Exception(msg);
        }
        
        private static async Task<ReadonlySignal<T>> GetReadonlySignal(string id)
        {
            var startTime = DateTime.Now;
            var timeout = EventDrivenHelper.Settings.SignalGetterTimeout;
            var aliveToken = TaskHelper.ApplicationAliveToken;
            string msg;
            
            while (!aliveToken.IsCancellationRequested && (DateTime.Now - startTime).TotalSeconds < timeout)
            {
                await Task.Yield();
                
                var arg = Signals.TryGetSignal(id, out Signal<T> signal);
                switch (arg)
                {
                    case 0:
                        return new ReadonlySignal<T>(signal);
                    case 2:
                        msg = $"Get Signal [{id}] fail: wrong type";
                        throw new Exception(msg);
                }
            }
            
            msg = $"Signal [{id}] is not found";
            throw new Exception(msg);
        }

        #endregion
        
        public string Id { get; }
        private readonly Observers<T> _observers;
        private T _value;
        private readonly bool _readonlyToObserver;
        
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                Trigger();
            }
        }
        
        protected override void DisposeManagedResources()
        {
            Signals.UnRegistrySignal(this);
            _observers.Dispose();
        }

        public void AddListener(Action<T> listener) =>
            _observers.AddListener(listener);

        public void RemoveListener(Action<T> listener) =>
            _observers.RemoveListener(listener);

        private void Trigger() => 
            _observers.Trigger(Value);

        public override string ToString() => Value.ToString();

        public bool Equals(T other) => 
            Comparer<T>.Default.Compare(Value, other) == 0;

        public bool Equals(Signal<T> other) => 
            other != null && Comparer<T>.Default.Compare(Value, other.Value) == 0;

        public bool Equals(ReadonlySignal<T> other) => 
            other != null && Comparer<T>.Default.Compare(Value, other.Value) == 0;

        public int CompareTo(Signal<T> other) =>
            other == null ? 1 : Comparer<T>.Default.Compare(Value, other.Value);

        public int CompareTo(ReadonlySignal<T> other) =>
            other == null ? 1 : Comparer<T>.Default.Compare(Value, other.Value);

        public int CompareTo(T other) =>
            other == null ? 1 : Comparer<T>.Default.Compare(Value, other);
            
        public static implicit operator T(Signal<T> signal) => signal.Value;
    }
}
