using AceLand.EventDriven.EventSignal;
using AceLand.EventDriven.EventSignal.Core;
using AceLand.EventDriven.ProjectSetting;
using AceLand.TasksUtils;
using AceLand.TasksUtils.Models;
using AceLand.TasksUtils.Promise;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AceLand.EventDriven.Handler
{
    public class ReadonlySignalGetter<T> : PromiseHandler<ReadonlySignalGetter<T>, ReadonlySignal<T>, ErrorMessage>
    {
        private static EventDrivenSettings Settings => EventDrivenHelper.Settings;
        
        internal ReadonlySignalGetter(string id)
        {
            GetSignal(id);
        }

        private void GetSignal(string id)
        {
            var targetTime = Time.realtimeSinceStartup + Settings.SignalGetterTimeout;
            var aliveToken = TaskHandler.ApplicationAliveToken;
            UniTask.Create(async () =>
            {
                var arg = 1;
                Signal<T> signal = default;
                while (Time.realtimeSinceStartup < targetTime)
                {
                    await UniTask.Yield(aliveToken);
                    if (aliveToken.IsCancellationRequested) return;
                    
                    arg = Signals.TryGetSignal(id, out signal);
                    if (arg is 0 or 2) break;
                }

                switch (arg)
                {
                    case 0:
                        OnSuccess?.Invoke(signal);
                        break;
                    case 1:
                        var msg = ErrorMessage.Builder()
                            .WithMessage("GetSignal", $"id [{id}] not found")
                            .Build();
                        OnError?.Invoke(msg);
                        break;
                    case 2:
                        msg = ErrorMessage.Builder()
                            .WithMessage("GetSignal", "incorrect type")
                            .Build();
                        OnError?.Invoke(msg);
                        break;
                }
                
                OnFinal?.Invoke();
            });
        }
    }
}