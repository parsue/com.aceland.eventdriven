﻿using System;
using System.Collections.Generic;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.EventDriven.Handler;
using AceLand.Library.Disposable;
using AceLand.Library.Optional;

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
        
        public static SignalGetter<Signal> Get(string id) => new (id); 
        
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