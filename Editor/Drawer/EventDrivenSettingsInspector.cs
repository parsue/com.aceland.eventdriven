using AceLand.EventDriven.ProjectSetting;
using AceLand.Library.Editor;
using UnityEditor;

namespace AceLand.EventDriven.Editor.Drawer
{
    [CustomEditor(typeof(EventDrivenSettings))]
    public class EventDrivenSettingsInspector : UnityEditor.Editor
    {        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorHelper.DrawAllPropertiesAsDisabled(serializedObject);
        }
    }
}