using AceLand.EventDriven.ProjectSetting;
using UnityEngine;

namespace AceLand.EventDriven.Core
{
    internal static class EventDrivenUtils
    {
        public static EventDrivenSettings Settings
        {
            get
            {
                _settings ??= Resources.Load<EventDrivenSettings>(nameof(EventDrivenSettings));
                return _settings;
            }
        }
        
        private static EventDrivenSettings _settings;
    }
}