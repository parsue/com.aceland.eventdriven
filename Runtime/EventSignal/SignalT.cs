using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Disposable;
using AceLand.Library.Optional;
using AceLand.TaskUtils;
using AceLand.TaskUtils.PromiseAwaiter;
using UnityEngine;

namespace AceLand.EventDriven.EventSignal
{
    public class Signal<T> : DisposableObject, ISignal<T>
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
            ISignalBuilder WithId<TEnum>(TEnum id) where TEnum : Enum;
            ISignalBuilder WithValue(T value);
            ISignalBuilder WithListener(Action<T> listener);
            ISignalBuilder WithListeners(params Action<T>[] listeners);
            ISignalBuilder ReadonlyToObserver();
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

            public ISignalBuilder WithId<TEnum>(TEnum id) where TEnum : Enum
            {
                _id = id.ToString().ToOption();
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

            public ISignalBuilder ReadonlyToObserver()
            {
                _readonlyToObserver = true;
                return this;
            }
        }

        #endregion

        #region Getter

        public static Promise<Signal<T>> Get(string id) =>
            GetSignal(id); 
        public static Promise<Signal<T>> Get<TEnum>(TEnum id) where TEnum: Enum =>
            GetSignal(id.ToString()); 
        public static Promise<ReadonlySignal<T>> GetReadonly(string id) =>
            GetReadonlySignal(id);
        public static Promise<ReadonlySignal<T>> GetReadonly<TEnum>(TEnum id) where TEnum: Enum =>
            GetReadonlySignal(id.ToString());

        private static async Task<Signal<T>> GetSignal(string id)
        {
            var aliveToken = TaskHelper.ApplicationAliveToken;
            var targetTime = Time.realtimeSinceStartup + EventDrivenHelper.Settings.SignalGetterTimeout;
            string msg;

            while (!aliveToken.IsCancellationRequested && Time.realtimeSinceStartup < targetTime)
            {
                await Task.Yield();
                
                var arg = Signals.TryGetSignal(id, out Signal<T> signal);
                switch (arg)
                {
                    case 0:
                        if (!signal._readonlyToObserver) return signal;
                        msg =
                            $"Get Signal [{id}] fail: marked as Readonly To Observer.  Use GetReadonly instead.";
                        throw new Exception(msg);

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
            var aliveToken = TaskHelper.ApplicationAliveToken;
            var targetTime = Time.realtimeSinceStartup + EventDrivenHelper.Settings.SignalGetterTimeout;
    
            while (!aliveToken.IsCancellationRequested && Time.realtimeSinceStartup < targetTime)
            {
                await Task.Yield();
                
                var arg = Signals.TryGetSignal(id, out Signal<T> signal);
                switch (arg)
                {
                    case 0:
                        return new ReadonlySignal<T>(signal);
                    case 2:
                        throw new Exception($"Get Signal [{id}] fail: wrong type");
                }
            }
    
            throw new Exception($"Signal [{id}] is not found");
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

        public void Trigger() => 
            _observers.Trigger(Value);

        public override string ToString() => Value.ToString();
        
        public bool CompareTo(T other) => 
            other != null && Comparer<T>.Default.Compare(Value, other) == 0;
        
        public bool CompareTo(Signal<T> other) => 
            other != null && Comparer<T>.Default.Compare(Value, other.Value) == 0;

        public bool CompareTo(ReadonlySignal<T> other) => 
            other != null && Comparer<T>.Default.Compare(Value, other.Value) == 0;
            
        public static implicit operator T(Signal<T> signal) => signal.Value;
    }
}
