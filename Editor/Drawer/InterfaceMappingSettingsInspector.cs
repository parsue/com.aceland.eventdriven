using AceLand.Library.Editor;
using AceLand.EventDriven.ProjectSetting;
using UnityEditor;

namespace Editor.Drawer
{
    [CustomEditor(typeof(EventDrivenSettings))]
    public class InterfaceMappingSettingsInspector : UnityEditor.Editor
    {        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorHelper.DrawAllPropertiesAsDisabled(serializedObject);
        }
    }
}