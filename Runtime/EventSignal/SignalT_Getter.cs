using System;
using System.Threading.Tasks;
using AceLand.EventDriven.Core;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.EventDriven.Exceptions;

namespace AceLand.EventDriven.EventSignal
{
    public static partial class Signal<T>
    {
        public static ISignal<T> GetOrCreate(string id)
        {
            return Signals.TryGetSignal(id, out SignalObject<T> s) == 0
                ? s
                : Signal.Builder().WithId(id).WithValue<T>().Build();
        }
        
        public static bool TryGet(string id, out ISignal<T> signal)
        {
            if (Signals.TryGetSignal(id, out SignalObject<T> s) != 0)
            {
                signal = null;
                return false;
            } 
            
            signal = s;
            return true;
        }
        
        public static bool TryGetReadonly(string id, out IReadonlySignal<T> readonlySignal)
        {
            if (Signals.TryGetSignal(id, out SignalObject<T> s) != 0)
            {
                readonlySignal = null;
                return false;
            } 
            
            readonlySignal = s.AsReadonly();
            return true;
        }

        public static bool TryGetListener(string id, out ISignalListener<T> listener)
        {
            if (Signals.TryGetSignal(id, out SignalObject<T> s) != 0)
            {
                listener = null;
                return false;
            } 
            
            listener = s.AsListener();
            return true;
        }
        
        public static bool TryGetTrigger(string id, out ISignalTrigger<T> trigger)
        {
            if (Signals.TryGetSignal(id, out SignalObject<T> s) != 0)
            {
                trigger = null;
                return false;
            } 
            
            trigger = s.AsTrigger();
            return true;
        }
        
        public static ISignal<T> GetOrCreate<TEnum>(TEnum id) =>
            GetOrCreate(id.ToString());
        
        public static bool TryGet<TEnum>(TEnum id, out ISignal<T> signal) where TEnum: Enum =>
            TryGet(id.ToString(), out signal);
        public static bool TryGetReadonly<TEnum>(TEnum id, out IReadonlySignal<T> readonlySignal) where TEnum: Enum =>
            TryGetReadonly(id.ToString(), out readonlySignal);
        public static bool TryGetListener<TEnum>(TEnum id, out ISignalListener<T> listener) where TEnum: Enum =>
            TryGetListener(id.ToString(), out listener);
        public static bool TryGetTrigger<TEnum>(TEnum id, out ISignalTrigger<T> trigger) where TEnum: Enum =>
            TryGetTrigger(id.ToString(), out trigger);

        public static Task<ISignal<T>> GetAsync(string id) =>
            GetSignalAsync(id); 
        public static Task<IReadonlySignal<T>> GetReadonlyAsync(string id) =>
            GetReadonlySignalAsync(id);
        public static Task<ISignalListener<T>> GetListenerAsync(string id) =>
            GetSignalListenerAsync(id);
        public static Task<ISignalTrigger<T>> GetTriggerAsync(string id) =>
            GetSignalTriggerAsync(id);
        
        public static Task<ISignal<T>> GetAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetAsync(id.ToString()); 
        public static Task<IReadonlySignal<T>> GetReadonlyAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetReadonlyAsync(id.ToString());
        public static Task<ISignalListener<T>> GetListenerAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetListenerAsync(id.ToString());
        public static Task<ISignalTrigger<T>> GetTriggerAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetTriggerAsync(id.ToString());

        private static async Task<ISignal<T>> GetSignalAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var targetTime = DateTime.Now.AddSeconds(EventDrivenUtils.SignalGetterTimeout);
            string msg;

            while (UnityEngine.Application.isPlaying && DateTime.Now < targetTime)
            {
                await Task.Yield();

                var arg = Signals.TryGetSignal(id, out SignalObject<T> signal);
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
