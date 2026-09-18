using System;
using System.Collections.Generic;
using AceLand.Lifecycle;
using ZLinq;

namespace AceLand.EventDriven.EventSignal.Core
{
    internal class SignalLinker : IDisposable, ISignalLinker
    {
        internal static SignalLinker Create(
            string id,
            SignalTriggerMethod triggerMethod,
            PlayerLoopPoint triggerState
        ) => new(id, triggerMethod, triggerState);
        
        private SignalLinker(
            string id,
            SignalTriggerMethod triggerMethod,
            PlayerLoopPoint triggerState
        )
        {
            Id = id;
            _triggerMethod = triggerMethod;
            _triggerState = triggerState;
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
        private readonly SignalTriggerMethod _triggerMethod;
        private readonly PlayerLoopPoint _triggerState;
        private bool _triggeredInThisFrame;

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

        public void AddListener(Action<bool> listener, bool runImmediately = false)
        {
            _observers.AddListener(listener);
            if (runImmediately) listener?.Invoke(GetResult());
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
            switch (_triggerMethod)
            {
                case SignalTriggerMethod.Immediately:
                    _observers.Trigger(GetResult());
                    return;
                
                case SignalTriggerMethod.OncePerFrame:
                    if (_triggeredInThisFrame) return;
                    _triggeredInThisFrame = true;
                    LifecycleFrame.RunEveryFrame(SystemTrigger, _triggerState, maxFrames: 1);
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SystemTrigger()
        {
            _observers.Trigger(GetResult());
            _triggeredInThisFrame = false;
        }

        private bool GetResult()
        {
            return adaptors.Values.AsValueEnumerable().All(a => a.Result());
        }
    }
}