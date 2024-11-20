using AceLand.Library.ProjectSetting;
using System;
using UnityEngine;

namespace AceLand.EventDriven.ProjectSetting
{
    public class EventDrivenSettings : ProjectSettings<EventDrivenSettings>
    {
        [Header("Interface Mapping")]
        [SerializeField] private string[] acceptedNamespaces = { "AceLand" };

        [Header("Signal")] 
        [SerializeField] private bool swapSignalOnSameId;
        [SerializeField, Min(0.1f)] private float signalGetterTimeout = 1.5f;

        public int AcceptedNamespaceCount => acceptedNamespaces.Length;
        public ReadOnlySpan<string> AcceptedNamespaces => acceptedNamespaces;

        public bool SwapSignalOnSameId => swapSignalOnSameId;
        public float SignalGetterTimeout => signalGetterTimeout;
    }
}