using System;
using System.Collections.Generic;
using AceLand.Disposable;
using ZLinq;

namespace AceLand.EventDriven.EventSignal.Core
{
    internal interface ISignalAdaptor
    {
        void Dispose();
        void AddAdaptor<T>(ISignalListener<T> signalListener, Predicate<T> result, Action linkerTrigger);
        AdaptorOption Option { get; }
        bool Result();
    }
    
    internal class SignalAdaptor : DisposableObject, ISignalAdaptor
    {
        internal static SignalAdaptor Create<T>(AdaptorOption option)
        {
            var adaptor = new SignalAdaptor(option);
            return adaptor;
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            foreach (var adaptor in adaptorData)
                adaptor.Dispose();
        }

        private SignalAdaptor(AdaptorOption option)
        {
            Option = option;
        }

        private readonly List<IAdaptorData> adaptorData = new();

        public AdaptorOption Option { get; }

        public void AddAdaptor<T>(ISignalListener<T> signalListener, Predicate<T> result, Action linkerTrigger)
        {
            var adaptor = AdaptorData<T>.Create(signalListener, result);
            adaptor.AddLinkerTrigger(linkerTrigger);
            adaptorData.Add(adaptor);
        }

        public bool Result()
        {
            return Option switch
            {
                AdaptorOption.And => adaptorData.AsValueEnumerable().All(a => a.Result()),
                AdaptorOption.Or => adaptorData.AsValueEnumerable().Any(a => a.Result()),
                _ => false
            };
        }
    }
}