using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AceLand.EventDriven.Editor
{
    internal static class AnalyzerImportFixer
    {
        private const string ANALYZER_NAME = "AceLand.EventDriven.SourceGenerator.dll";
        private const string LABEL = "RoslynAnalyzer";

        [MenuItem("Tools/AceLand/EventDriven/Fix Analyzer Import Settings", false, 43)]
        private static void Fix()
        {
            var path = FindAnalyzer();
            if (path == null)
            {
                Debug.LogError($"[EventDriven] {ANALYZER_NAME} not found. " +
                               "Build it: SourceGenerator~ → dotnet build -c Release");
                return;
            }

            var importer = AssetImporter.GetAtPath(path) as PluginImporter;
            if (!importer)
            {
                Debug.LogError($"[EventDriven] {path} is not imported as a plugin. " +
                               "Is it inside an immutable package? Fix the .meta in the repo instead.");
                return;
            }

            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(false);
            foreach (var target in (BuildTarget[])System.Enum.GetValues(typeof(BuildTarget)))
            {
                if ((int)target < 0) continue;                        // obsolete entries
                try { importer.SetCompatibleWithPlatform(target, false); }
                catch { /* unsupported target on this install */ }
            }

            if (PluginImporterCompat.TrySet(importer, false, out var how))
                Debug.Log($"[EventDriven] validateReferences = false (via {how})");
            else
                Debug.LogWarning(
                    "[EventDriven] Could not set validateReferences from script on this Unity version.\n" +
                    "The RoslynAnalyzer label alone is usually enough. If the 'Unable to resolve reference' " +
                    "error persists, edit the .meta manually — see the package README.");

            var labels = AssetDatabase.GetLabels(importer).ToList();
            if (!labels.Contains(LABEL))
            {
                labels.Add(LABEL);
                AssetDatabase.SetLabels(importer, labels.ToArray());
            }

            importer.SaveAndReimport();

            Debug.Log($"[EventDriven] Fixed import settings for {path}.\n" +
                      "→ Restart Unity to let the source generator run.");
        }

        private static string FindAnalyzer()
        {
            var candidates = new[]
            {
                "Packages/com.aceland.eventdriven/Analyzers/" + ANALYZER_NAME,
                "Assets/Plugins/AceLand/" + ANALYZER_NAME,
            };

            foreach (var c in candidates)
                if (File.Exists(c)) return c;

            return AssetDatabase.FindAssets("AceLand.EventDriven.SourceGenerator")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => p.EndsWith(ANALYZER_NAME, System.StringComparison.Ordinal));
        }
    }
}
