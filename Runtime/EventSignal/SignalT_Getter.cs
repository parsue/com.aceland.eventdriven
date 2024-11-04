using System;
using System.Threading.Tasks;
using AceLand.EventDriven.Core;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.TaskUtils;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T>
    {
        public static Task<Signal<T>> Get(string id) =>
            GetSignal(id); 
        public static Task<Signal<T>> Get<TEnum>(TEnum id) where TEnum: Enum =>
            GetSignal(id.ToString()); 
        public static Task<ReadonlySignal<T>> GetReadonly(string id) =>
            GetReadonlySignal(id);
        public static Task<ReadonlySignal<T>> GetReadonly<TEnum>(TEnum id) where TEnum: Enum =>
            GetReadonlySignal(id.ToString());

        private static async Task<Signal<T>> GetSignal(string id)
        {
            var aliveToken = Promise.ApplicationAliveToken;
            var targetTime = DateTime.Now.AddSeconds(EventDrivenUtils.Settings.SignalGetterTimeout);
            string msg;

            while (!aliveToken.IsCancellationRequested && DateTime.Now < targetTime)
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
            var aliveToken = Promise.ApplicationAliveToken;
            var targetTime = DateTime.Now.AddSeconds(EventDrivenUtils.Settings.SignalGetterTimeout);
    
            while (!aliveToken.IsCancellationRequested && DateTime.Now < targetTime)
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
    }
}
