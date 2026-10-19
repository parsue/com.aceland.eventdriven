using AceLand.EventDriven.Core;
using UnityEditor;
using UnityEngine;

namespace AceLand.EventDriven.Editor.Settings
{
    /// <summary>
    /// Project-scoped editor settings for EventDriven, persisted to
    /// <c>ProjectSettings/AceLandEventDriven.asset</c> via <see cref="ScriptableSingleton{T}"/>.
    ///
    /// ScriptableSingleton is editor-only. The runtime holds its own value directly
    /// (zero dependency); on load / on change we push <see cref="signalGetterTimeout"/>
    /// into the runtime through <see cref="EventDrivenEditorConfig"/>.
    /// </summary>
    [FilePath("ProjectSettings/AceLandEventDriven.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class EventDrivenEditorSettings : ScriptableSingleton<EventDrivenEditorSettings>
    {
        [Tooltip("Timeout (seconds) used by signal getters while waiting for a signal to appear. " +
                 "Must be greater than 0; otherwise the runtime default is used.")]
        public float signalGetterTimeout = EventDrivenEditorConfig.DefaultSignalGetterTimeout;

        public void Save() => Save(true);

        /// <summary>Push the configured value into the runtime.</summary>
        public void ApplyToRuntime() =>
            EventDrivenEditorConfig.SetSignalGetterTimeout(signalGetterTimeout);
    }

    /// <summary>
    /// Injects the saved settings into the runtime once per domain load, so the
    /// configured <c>SignalGetterTimeout</c> is in effect for edit-mode play and
    /// before entering play mode.
    /// </summary>
    [InitializeOnLoad]
    internal static class EventDrivenEditorSettingsBootstrap
    {
        static EventDrivenEditorSettingsBootstrap() =>
            EventDrivenEditorSettings.instance.ApplyToRuntime();
    }

    internal static class EventDrivenSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create() =>
            new("Project/AceLand Packages/EventDriven", SettingsScope.Project)
            {
                label = "EventDriven",
                guiHandler = _ =>
                {
                    var s = EventDrivenEditorSettings.instance;
                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.LabelField("Signal", EditorStyles.boldLabel);
                    var value = EditorGUILayout.FloatField(
                        new GUIContent("Signal Getter Timeout",
                            "Seconds a signal getter waits for a signal to appear before timing out. " +
                            $"Default: {EventDrivenEditorConfig.DefaultSignalGetterTimeout:0.##}s."),
                        s.signalGetterTimeout);
                    s.signalGetterTimeout = value > 0f
                        ? value
                        : EventDrivenEditorConfig.DefaultSignalGetterTimeout;

                    if (EditorGUI.EndChangeCheck())
                    {
                        s.Save();
                        s.ApplyToRuntime();
                    }

                    EditorGUILayout.Space(8);
                    if (GUILayout.Button("Reset To Default", GUILayout.Width(140)))
                    {
                        s.signalGetterTimeout = EventDrivenEditorConfig.DefaultSignalGetterTimeout;
                        s.Save();
                        s.ApplyToRuntime();
                    }
                },
                keywords = new[] { "aceland", "eventdriven", "event", "signal", "timeout" }
            };
    }
}
