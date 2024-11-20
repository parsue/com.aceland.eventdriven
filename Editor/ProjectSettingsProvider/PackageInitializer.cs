using AceLand.EventDriven.ProjectSetting;
using UnityEditor;

namespace AceLand.EventDriven.Editor.ProjectSettingsProvider
{
    [InitializeOnLoad]
    public static class PackageInitializer
    {
        static PackageInitializer()
        {
            EventDrivenSettings.GetSerializedSettings();
        }
    }
}