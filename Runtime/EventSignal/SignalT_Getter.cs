﻿using System;
using System.Threading.Tasks;
using AceLand.EventDriven.Core;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.Library.Disposable;
using AceLand.TaskUtils;
using UnityEngine;

namespace AceLand.EventDriven.EventSignal
{
    public partial class Signal<T>
    {
        public static Promise<Signal<T>> Get(string id) =>
            GetSignal(id); 
        public static Promise<Signal<T>> Get<TEnum>(TEnum id) where TEnum: Enum =>
            GetSignal(id.ToString()); 
        public static Promise<ReadonlySignal<T>> GetReadonly(string id) =>
            GetReadonlySignal(id);
        public static Promise<ReadonlySignal<T>> GetReadonly<TEnum>(TEnum id) where TEnum: Enum =>
            GetReadonlySignal(id.ToString());

        private static async Task<Signal<T>> GetSignal(string id)
        {
            var aliveToken = Promise.ApplicationAliveToken;
            var targetTime = Time.realtimeSinceStartup + EventDrivenUtils.Settings.SignalGetterTimeout;
            string msg;

            while (!aliveToken.IsCancellationRequested && Time.realtimeSinceStartup < targetTime)
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
            var targetTime = Time.realtimeSinceStartup + EventDrivenUtils.Settings.SignalGetterTimeout;
    
            while (!aliveToken.IsCancellationRequested && Time.realtimeSinceStartup < targetTime)
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