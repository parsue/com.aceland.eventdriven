using System;
using AceLand.Library.Disposable;
using UnityEngine;

namespace AceLand.EventDriven.EventSignal.Core
{
    internal interface IAdaptorData
    {
        void Dispose();
        void AddLinkerTrigger(Action linkerTrigger);
        bool Result();
    }

    internal class AdaptorData<T> : DisposableObject, IAdaptorData
    {
        internal static AdaptorData<T> Create(ISignalListener<T> signalListener, Predicate<T> result) =>
            new(signalListener, result);
        
        private AdaptorData(ISignalListener<T> listener, Predicate<T> getResult)
        {
            Listener = listener;
            GetResult = getResult;
        }

        ~AdaptorData()
        {
            Dispose(false);
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            Listener?.RemoveListener(trigger);
        }

        public void AddLinkerTrigger(Action linkerTrigger)
        {
            trigger = _ => linkerTrigger?.Invoke();
            Listener?.AddListener(trigger);
        }

        private ISignalListener<T> Listener { get; }
        private Predicate<T> GetResult { get; }
        private Action<T> trigger;

        public bool Result()
        {
            if (Listener == null)
            {
                Debug.LogWarning("Single Linker: Signal in Linker is not exist. Condition of current signal will always true.");
                return true;
            }
            
            if (Listener.Disposed)
            {
                Debug.LogWarning($"Single Linker: Signal [{Listener.Id ?? ""}] was disposed. Condition of current signal will always true.");
                return true;
            }
            
            return GetResult?.Invoke(Listener.RefSignal.Value) ?? false;
        }
    }
}