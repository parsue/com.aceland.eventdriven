using AceLand.EventDriven.ProjectSetting;
using AceLand.PlayerLoopHack;
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

        public static PlayerLoopState PreviousState(PlayerLoopState state)
        {
            if (state is PlayerLoopState.TimeUpdate)
                return PlayerLoopState.PostLateUpdate;

            return state - 1;
        }
    }
}