using System;
using System.Threading.Tasks;
using AceLand.EventDriven.Core;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.EventDriven.Exceptions;
using AceLand.TaskUtils;
using UnityEngine;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal
    {
        public static bool TryGet(string id, out ISignal signal)
        {
            if (Signals.TryGetSignal(id, out Signal s) != 0)
            {
                signal = null;
                return false;
            } 
            
            signal = s;
            return true;
        }
        
        public static bool TryGetLinker(string id, out ISignalLinker signal)
        {
            if (Signals.TryGetSignal(id, out SignalLinker s) != 0)
            {
                signal = null;
                return false;
            } 
            
            signal = s;
            return true;
        }

        public static bool TryGetListener(string id, out ISignalListener listener)
        {
            if (Signals.TryGetSignal(id, out Signal s) != 0)
            {
                listener = null;
                return false;
            } 
            
            listener = s.AsListener();
            return true;
        }
        
        public static bool TryGetTrigger(string id, out ISignalTrigger trigger)
        {
            if (Signals.TryGetSignal(id, out Signal s) != 0)
            {
                trigger = null;
                return false;
            } 
            
            trigger = s.AsTrigger();
            return true;
        }
        
        public static bool TryGet<TEnum>(TEnum id, out ISignal signal) where TEnum: Enum =>
            TryGet(id.ToString(), out signal);
        public static bool TryGetLinker<TEnum>(TEnum id, out ISignalLinker signal) where TEnum: Enum =>
            TryGetLinker(id.ToString(), out signal);
        public static bool TryGetListener<TEnum>(TEnum id, out ISignalListener listener) where TEnum: Enum =>
            TryGetListener(id.ToString(), out listener);
        public static bool TryGetTrigger<TEnum>(TEnum id, out ISignalTrigger trigger) where TEnum: Enum =>
            TryGetTrigger(id.ToString(), out trigger);

        public static Promise<ISignal> GetAsync(string id) => 
            GetSignalAsync(id); 
        public static Promise<ISignalLinker> GetLinkerAsync(string id) => 
            GetSignalLinkerAsync(id); 
        public static Promise<ISignalListener> GetListenerAsync(string id) =>
            GetSignalListenerAsync(id);
        public static Promise<ISignalTrigger> GetTriggerAsync(string id) =>
            GetSignalTriggerAsync(id);
        
        public static Promise<ISignal> GetAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsync(id.ToString());
        public static Promise<ISignalLinker> GetLinkerAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetLinkerAsync(id.ToString());
        public static Promise<ISignalListener> GetListenerAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetListenerAsync(id.ToString());
        public static Promise<ISignalTrigger> GetTriggerAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetTriggerAsync(id.ToString());
        
        private static async Task<ISignal> GetSignalAsync(string id)
        {
            return await GetSignalTAsync<Signal>(id);
        }
        
        private static async Task<ISignalLinker> GetSignalLinkerAsync(string id)
        {
            return await GetSignalTAsync<SignalLinker>(id);
        }
        
        private static async Task<ISignalListener> GetSignalListenerAsync(string id)
        {
            var signal = await GetSignalTAsync<Signal>(id);
            return signal.AsListener();
        }
        
        private static async Task<ISignalTrigger> GetSignalTriggerAsync(string id)
        {
            var signal = await GetSignalTAsync<Signal>(id);
            return signal.AsTrigger();
        }

        private static async Task<T> GetSignalTAsync<T>(string id)
            where T : IEventSignal
        {
            var aliveToken = Promise.ApplicationAliveToken;
            var targetTime = Time.realtimeSinceStartup + EventDrivenUtils.Settings.SignalGetterTimeout;
            string msg;
    
            while (!aliveToken.IsCancellationRequested && Time.realtimeSinceStartup < targetTime)
            {
                var arg = Signals.TryGetSignal(id, out T signal);
                
                switch (arg)
                {
                    case 0:
                        return signal;
                    case 2:
                        msg = $"Get Signal [{id}] fail: wrong type";
                        throw new SignalTypeErrorException(msg);
                }

                await Task.Yield();
            }

            msg = $"Signal [{id}] is not found";
            throw new SignalNotFoundException(msg);
        }
    }
}