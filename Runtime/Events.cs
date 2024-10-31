using AceLand.EventDriven.ProjectSetting;
using UnityEngine;

namespace AceLand.EventDriven
{
    public static class Events
    {
        public static EventDrivenSettings Settings
        {
            get => _settings ?? Resources.Load<EventDrivenSettings>(nameof(EventDrivenSettings));
            internal set => _settings = value;
        }
        
        private static EventDrivenSettings _settings;
    }
}