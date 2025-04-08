using AceLand.Library.ProjectSetting;
using UnityEngine;

namespace AceLand.EventDriven.ProjectSetting
{
    public class EventDrivenSettings : ProjectSettings<EventDrivenSettings>
    {
        [Header("Signal")] 
        [SerializeField] private bool swapSignalOnSameId;
        [SerializeField, Min(0.1f)] private float signalGetterTimeout = 1.5f;

        public bool SwapSignalOnSameId => swapSignalOnSameId;
        public float SignalGetterTimeout => signalGetterTimeout;
    }
}