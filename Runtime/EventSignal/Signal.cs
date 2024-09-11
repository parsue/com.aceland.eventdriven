using System;
using System.Collections.Generic;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Disposable;
using AceLand.Library.Optional;
using AceLand.TaskUtils;
using AceLand.TaskUtils.PromiseAwaiter;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AceLand.EventDriven.EventSignal
{
    public class Signal : DisposableObject, ISignal
    {
        private Signal(string id, Observers observers)
        {
            Id = id;
            _observers = observers;
        }

        #region Builder

        public static ISignalBuilder Builder() => new SignalBuilder();
        
        public interface ISignalBuilder
        {
            Signal Build();
            ISignalBuilder WithId(string id);
            ISignalBuilder WithListener(Action listener);
            ISignalBuilder WithListeners(params Action[] listeners);
        }

        private class SignalBuilder : ISignalBuilder
        {
            private Option<string> _id = Option<string>.None();
            private readonly List<Action> _listeners = new();

            public Signal Build()
            {
                var id = _id.Reduce(Guid.NewGuid().ToString);
                var observers = new Observers(_listeners.ToArray());
                var signal = new Signal(id, observers);
                Signals.RegistrySignal(signal);
                return signal;
            }

            public ISignalBuilder WithId(string id)
            {
                _id = id.ToOption();
                return this;
            }

            public ISignalBuilder WithListener(Action listener)
            {
                _listeners.Add(listener);
                return this;
            }

            public ISignalBuilder WithListeners(params Action[] listeners)
            {
                _listeners.AddRange(listeners);
                return this;
            }
        }

        #endregion

        #region Getter

        public static Promise<Signal> Get(string id) => GetSignal(id); 

        private static async UniTask<Signal> GetSignal(string id)
        {
            var targetTime = Time.realtimeSinceStartup + EventDrivenHelper.Settings.SignalGetterTimeout;
            var aliveToken = TaskHandler.ApplicationAliveToken;
            
            while (Time.realtimeSinceStartup < targetTime)
            {
                await UniTask.Yield();
                var arg = Signals.TryGetSignal(id, out Signal signal);
                switch (arg)
                {
                    case 0:
                        return signal;
                    case 2:
                        throw new Exception($"Get Signal [{id}] fail: wrong type");
                }
            }
            
            throw new Exception($"Signal [{id}] is not found");
        }

        #endregion
        
        public string Id { get; }
        private readonly Observers _observers;

        protected override void DisposeManagedResources()
        {
            Signals.UnRegistrySignal(this);
            _observers.Dispose();
        }

        public void AddListener(Action listener) =>
            _observers.AddListener(listener);

        public void RemoveListener(Action listener) =>
            _observers.RemoveListener(listener);

        public void Trigger() => 
            _observers.Trigger();
    }
}