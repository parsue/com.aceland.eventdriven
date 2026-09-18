using System;
using AceLand.EventDriven.Profiles;
using AceLand.Lifecycle;
using AceLand.ProjectSetting;
using UnityEngine;

namespace AceLand.EventDriven.ProjectSetting
{
    public class EventDrivenSettings : ProjectSettings<EventDrivenSettings>
    {
        [Header("Signal")] 
        [SerializeField, Min(0.1f)] private float signalGetterTimeout = 1.5f;
        [SerializeField] private PlayerLoopPoint signalTriggerState = PlayerLoopPoint.EarlyUpdate;
        [SerializeField] private SignalProviderBase[] prewarmProviders = Array.Empty<SignalProviderBase>();

        internal float SignalGetterTimeout => signalGetterTimeout;
        internal PlayerLoopPoint SignalTriggerState => signalTriggerState;
        internal ReadOnlySpan<SignalProviderBase> PrewarmProviders => prewarmProviders;
    }
}