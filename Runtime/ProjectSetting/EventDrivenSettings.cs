using System;
using AceLand.EventDriven.Profiles;
using AceLand.Library.ProjectSetting;
using AceLand.PlayerLoopHack;
using UnityEngine;

namespace AceLand.EventDriven.ProjectSetting
{
    public class EventDrivenSettings : ProjectSettings<EventDrivenSettings>
    {
        [Header("Signal")] 
        [SerializeField, Min(0.1f)] private float signalGetterTimeout = 1.5f;
        [SerializeField] private PlayerLoopState signalTriggerState = PlayerLoopState.EarlyUpdate;
        [SerializeField] private SignalProviderBase[] prewarmProviders = Array.Empty<SignalProviderBase>();

        internal float SignalGetterTimeout => signalGetterTimeout;
        internal PlayerLoopState SignalTriggerState => signalTriggerState;
        internal ReadOnlySpan<SignalProviderBase> PrewarmProviders => prewarmProviders;
    }
}