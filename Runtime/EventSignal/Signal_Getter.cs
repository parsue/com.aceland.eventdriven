using System;
using System.Threading.Tasks;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.TaskUtils;
using AceLand.TaskUtils.PromiseAwaiter;
using UnityEngine;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal
    {
        public static Promise<Signal> Get(string id) => 
            GetSignal(id); 
        public static Promise<Signal> Get<TEnum>(TEnum id) where TEnum: Enum =>
            GetSignal(id.ToString()); 

        private static async Task<Signal> GetSignal(string id)
        {
            var aliveToken = TaskHelper.ApplicationAliveToken;
            var targetTime = Time.realtimeSinceStartup + EventDrivenHelper.Settings.SignalGetterTimeout;
            string msg;
    
            while (!aliveToken.IsCancellationRequested && Time.realtimeSinceStartup < targetTime)
            {
                var arg = Signals.TryGetSignal(id, out Signal signal);
                
                switch (arg)
                {
                    case 0:
                        return signal;
                    case 2:
                        msg = $"Get Signal [{id}] fail: wrong type";
                        throw new Exception(msg);
                }

                await Task.Yield();
            }

            msg = $"Signal [{id}] is not found";
            throw new Exception(msg);
        }
    }
}