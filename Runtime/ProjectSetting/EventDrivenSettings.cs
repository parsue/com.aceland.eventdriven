using AceLand.Library.ProjectSetting;
using System;
using System.Linq;
using AceLand.Library.BuildLeveling;
using UnityEngine;

namespace AceLand.EventDriven.ProjectSetting
{
    public class EventDrivenSettings : ProjectSettings<EventDrivenSettings>
    {
        [Header("Interface Binding")]
        [SerializeField] private string[] acceptedNamespaces = { "AceLand" };
        [SerializeField, Min(0.1f)] private float bindingGetterTimeout = 1.5f;
        
        [Header("Signal")] 
        [SerializeField] private bool swapSignalOnSameId;
        [SerializeField, Min(0.1f)] private float signalGetterTimeout = 1.5f;

        public int AcceptedNamespaceCount => acceptedNamespaces.Length;
        public ReadOnlySpan<string> AcceptedNamespaces => acceptedNamespaces;
        
        public float BindingGetterTimeout => bindingGetterTimeout;

        public bool SwapSignalOnSameId => swapSignalOnSameId;
        public float SignalGetterTimeout => signalGetterTimeout;

        public bool IsAcceptedNamespace(string fullNamespace) =>
            acceptedNamespaces.Any(fullNamespace.StartsWith);
    }
}