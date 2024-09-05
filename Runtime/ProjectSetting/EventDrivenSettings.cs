using AceLand.Library.ProjectSetting;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AceLand.EventDriven.ProjectSetting
{
    public class EventDrivenSettings : ProjectSettings<EventDrivenSettings>
    {
        [Header("Interface Mapping")]
        [SerializeField] private string[] acceptedNamespaces = new[] { "AMVR" };

        [FormerlySerializedAs("swapSignalOnSameName")]
        [Header("Signal")] 
        [SerializeField] private bool swapSignalOnSameId;
        [SerializeField] private float signalGetterTimeout = 1.5f;

        public ReadOnlySpan<string> AcceptedNamespaces => acceptedNamespaces;

        public bool SwapSignalOnSameId => swapSignalOnSameId;
        public float SignalGetterTimeout => signalGetterTimeout;
    }
}