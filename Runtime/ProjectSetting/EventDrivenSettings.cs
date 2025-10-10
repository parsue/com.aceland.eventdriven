using System;
using AceLand.EventDriven.Profiles;
using AceLand.Library.ProjectSetting;
using UnityEngine;

namespace AceLand.EventDriven.ProjectSetting
{
    public class EventDrivenSettings : ProjectSettings<EventDrivenSettings>
    {
        [Header("Event Signal")] 
        [SerializeField, Min(0.1f)] private float signalGetterTimeout = 1.5f;
        [SerializeField] private SignalProviderBase[] prewarmProviders = Array.Empty<SignalProviderBase>();

        internal float SignalGetterTimeout => signalGetterTimeout;
        internal ReadOnlySpan<SignalProviderBase> PrewarmProviders => prewarmProviders;
    }
}