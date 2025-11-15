using System;
using System.Threading.Tasks;
using AceLand.EventDriven.Core;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.EventDriven.Exceptions;
using AceLand.TaskUtils;
using UnityEngine;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T>
    {
        public static ISignal<T> Get(string id) =>
            Signals.TryGetSignal(id, out Signal<T> signal) == 0 ? signal : null;
        public static IReadonlySignal<T> GetAsReadonly(string id) =>
            Signals.TryGetSignal(id, out Signal<T> signal) == 0 ? signal.AsReadonly() : null;
        public static ISignalListener<T> GetAsListener(string id) =>
            Signals.TryGetSignal(id, out Signal<T> signal) == 0 ? signal.AsListener() : null;
        public static ISignalTrigger<T> GetAsTrigger(string id) =>
            Signals.TryGetSignal(id, out Signal<T> signal) == 0 ? signal.AsTrigger() : null;
        
        public static ISignal<T> Get<TEnum>(TEnum id) where TEnum: Enum =>
            Get(id.ToString());
        public static IReadonlySignal<T> GetAsReadonly<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsReadonly(id.ToString());
        public static ISignalListener<T> GetAsListener<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsListener(id.ToString());
        public static ISignalTrigger<T> GetAsTrigger<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsTrigger(id.ToString());

        public static Promise<ISignal<T>> GetAsync(string id) =>
            GetSignalAsync(id); 
        public static Promise<IReadonlySignal<T>> GetAsReadonlyAsync(string id) =>
            GetReadonlySignalAsync(id);
        public static Promise<ISignalListener<T>> GetAsListenerAsync(string id) =>
            GetSignalListenerAsync(id);
        public static Promise<ISignalTrigger<T>> GetAsTriggerAsync(string id) =>
            GetSignalTriggerAsync(id);
        
        public static Promise<ISignal<T>> GetAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsync(id.ToString()); 
        public static Promise<IReadonlySignal<T>> GetAsReadonlyAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsReadonlyAsync(id.ToString());
        public static Promise<ISignalListener<T>> GetAsListenerAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsListenerAsync(id.ToString());
        public static Promise<ISignalTrigger<T>> GetAsTriggerAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsTriggerAsync(id.ToString());

        private static async Task<ISignal<T>> GetSignalAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));
            
            var aliveToken = Promise.ApplicationAliveToken;
            var targetTime = DateTime.Now.AddSeconds(EventDrivenUtils.Settings.SignalGetterTimeout);
            string msg;

            while (!aliveToken.IsCancellationRequested && DateTime.Now < targetTime)
            {
                await Task.Yield();

                var arg = Signals.TryGetSignal(id, out Signal<T> signal);
                Debug.Log($"result: {arg}");
                switch (arg)
                {
                    case 0:
                        return signal;

                    case 2:
                        msg = $"Get Signal [{id}] fail: wrong type";
                        throw new SignalTypeErrorException(msg);

                    default:
                        continue;
                }
            }

            Debug.Log(99);
            msg = $"Signal [{id}] is not found";
            throw new SignalNotFoundException(msg);
        }
        
        private static async Task<IReadonlySignal<T>> GetReadonlySignalAsync(string id)
        {
            var signal = await GetSignalAsync(id);
            return signal.AsReadonly();
        }
        
        private static async Task<ISignalListener<T>> GetSignalListenerAsync(string id)
        {
            var signal = await GetSignalAsync(id);
            return signal.AsListener();
        }
        
        private static async Task<ISignalTrigger<T>> GetSignalTriggerAsync(string id)
        {
            var signal = await GetSignalAsync(id);
            return signal.AsTrigger();
        }
    }
}
