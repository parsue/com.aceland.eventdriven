using AceLand.EventDriven.ProjectSetting;
using AceLand.Library.Editor.Providers;
using UnityEditor;
using UnityEngine.UIElements;

namespace AceLand.EventDriven.Editor.ProjectSettingsProvider
{
    public class EventDrivenSettingsProvider : AceLandSettingsProvider
    {
        public const string SETTINGS_NAME = "Project/AceLand Event Driven";
        
        private EventDrivenSettingsProvider(string path, SettingsScope scope = SettingsScope.User) 
            : base(path, scope) { }
        
        [InitializeOnLoadMethod]
        public static void CreateSettings() => EventDrivenSettings.GetSerializedSettings();
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            Settings = EventDrivenSettings.GetSerializedSettings();
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new EventDrivenSettingsProvider(SETTINGS_NAME, SettingsScope.Project);
            return provider;
        }
    }
}