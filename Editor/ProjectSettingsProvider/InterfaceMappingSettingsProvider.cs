using AceLand.Library.Editor;
using AceLand.EventDriven.ProjectSetting;
using UnityEditor;
using UnityEngine.UIElements;

namespace AceLand.EventDriven.Editor.ProjectSettingsProvider
{
    public class InterfaceMappingSettingsProvider : SettingsProvider
    {
        public const string SETTINGS_NAME = "Project/AceLand Event Driven";
        private SerializedObject _settings;
        
        private InterfaceMappingSettingsProvider(string path, SettingsScope scope = SettingsScope.User) 
            : base(path, scope) { }
        
        [InitializeOnLoadMethod]
        public static void CreateSettings() => EventDrivenSettings.GetSerializedSettings();
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settings = EventDrivenSettings.GetSerializedSettings();
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new InterfaceMappingSettingsProvider(SETTINGS_NAME, SettingsScope.Project);
            return provider;
        }

        public override void OnGUI(string searchContext)
        {
            EditorHelper.DrawAllProperties(_settings);
        }
    }
}