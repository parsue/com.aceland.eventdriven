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
        public static Promise<Signal> GetAsync(string id) => 
            GetSignalAsync(id); 
        public static Promise<Signal> GetAsync<TEnum>(TEnum id) where TEnum: Enum =>
            GetSignalAsync(id.ToString());

        public static Signal Get(string id) =>
            Signals.TryGetSignal(id, out Signal signal) == 0 ? signal : null;

        public static Signal Get<TEnum>(TEnum id) where TEnum: Enum =>
            Get(id.ToString());

        private static async Task<Signal> GetSignalAsync(string id)
        {
            var aliveToken = Promise.ApplicationAliveToken;
            var targetTime = Time.realtimeSinceStartup + EventDrivenUtils.Settings.SignalGetterTimeout;
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
                        throw new SignalTypeErrorException(msg);
                }

                await Task.Yield();
            }

            msg = $"Signal [{id}] is not found";
            throw new SignalNotFoundException(msg);
        }
    }
}