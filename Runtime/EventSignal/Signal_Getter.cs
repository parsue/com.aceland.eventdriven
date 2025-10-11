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
        public static ISignal Get(string id) =>
            Signals.TryGetSignal(id, out Signal signal) == 0 ? signal : null;
        public static ISignalListener GetAsListener(string id) =>
            Signals.TryGetSignal(id, out Signal signal) == 0 ? signal.AsListener() : null;
        public static ISignalTrigger GetAsTrigger(string id) =>
            Signals.TryGetSignal(id, out Signal signal) == 0 ? signal.AsTrigger() : null;
        
        public static ISignal Get<TEnum>(TEnum id) where TEnum: Enum =>
            Get(id.ToString());
        public static ISignalListener GetAsListener<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsListener(id.ToString());
        public static ISignalTrigger GetAsTrigger<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsTrigger(id.ToString());

        public static Promise<ISignal> GetAsync(string id) => 
            GetSignalAsync(id); 
        public static Promise<ISignalListener> GetAsListenerAsync(string id) =>
            GetSignalListenerAsync(id);
        public static Promise<ISignalTrigger> GetAsTriggerAsync(string id) =>
            GetSignalTriggerAsync(id);
        
        public static Promise<ISignal> GetAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsync(id.ToString());
        public static Promise<ISignalListener> GetAsListenerAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsListenerAsync(id.ToString());
        public static Promise<ISignalTrigger> GetAsTriggerAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsTriggerAsync(id.ToString());

        private static async Task<ISignal> GetSignalAsync(string id)
        {
            var aliveToken = Promise.ApplicationAliveToken;
            var targetTime = Time.realtimeSinceStartup + EventDrivenUtils.Settings.SignalGetterTimeout;
            string msg;
    
            while (!aliveToken.IsCancellationRequested && Time.realtimeSinceStartup < targetTime)
            {
                var arg = Signals.TryGetSignal(id, out EventSignal.Signal signal);
                
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
        
        private static async Task<ISignalListener> GetSignalListenerAsync(string id)
        {
            var signal = await GetSignalAsync(id);
            return signal.AsListener();
        }
        
        private static async Task<ISignalTrigger> GetSignalTriggerAsync(string id)
        {
            var signal = await GetSignalAsync(id);
            return signal.AsTrigger();
        }
    }
}