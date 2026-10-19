using System;
using System.Collections.Generic;
using AceLand.EventDriven.Bus.Diagnostics;
using AceLand.EventDriven.Editor.Licensing;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AceLand.EventDriven.Editor.Monitor
{
    /// <summary>
    /// Per-event history window. Lists every captured <see cref="EventRaiseRecord"/> for a
    /// single event (newest first) — both raises and cache clears — with type, payload and
    /// caller source, and lets the user jump to the raising script line.
    /// </summary>
    internal sealed class EventRaiseHistoryWindow : EditorWindow
    {
        private const string ToolName = "AceLand Event Driven — Event History";

        private EventMonitorRow _row;
        private readonly List<EventRaiseRecord> _records = new();
        private MultiColumnListView _listView;
        private Label _emptyLabel;

        public static void Open(EventMonitorRow row)
        {
            if (row == null) return;
            var window = GetWindow<EventRaiseHistoryWindow>();
            window.titleContent = new GUIContent("Event History");
            window.minSize = new Vector2(520, 260);
            window.Bind(row);
            window.Show();
        }

        private void Bind(EventMonitorRow row)
        {
            _row = row;
            RebuildRoot();
        }

        private void OnEnable()
        {
            EventMonitorBridge.EventRaised += OnEventRaised;
            LicenseGate.Current.Changed += OnLicenseChanged;
            LicenseGate.Current.EnsureChecked();
            RebuildRoot();
        }

        private void OnDisable()
        {
            EventMonitorBridge.EventRaised -= OnEventRaised;
            LicenseGate.Current.Changed -= OnLicenseChanged;
        }

        // 依目前授權狀態重建視窗:未授權時以 IMGUIContainer 繪製引導頁取代工具內容,
        // 避免使用者繞過 Monitor 直接開啟 History 而取得付費功能。
        private void RebuildRoot()
        {
            rootVisualElement.Clear();

            if (!LicenseGate.Current.IsEntitled)
            {
                _listView = null;
                _emptyLabel = null;
                rootVisualElement.Add(new IMGUIContainer(() =>
                    LicenseGate.Current.DrawBlockedPage(ToolName))
                {
                    style = { flexGrow = 1, marginLeft = 6, marginRight = 6 }
                });
                return;
            }

            BuildUi();
            Refresh();
        }

        private void OnLicenseChanged()
        {
            LicenseGate.Current.EnsureChecked();
            RebuildRoot();
            Repaint();
        }

        private void BuildUi()
        {
            var header = new Label(_row != null ? (_row.DisplayName ?? _row.InterfaceName) : "—")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 13, marginTop = 6, marginLeft = 8, marginBottom = 4
                }
            };
            rootVisualElement.Add(header);

            var toolbar = new Toolbar();
            toolbar.Add(new ToolbarButton(Refresh) { text = "Refresh" });
            toolbar.Add(new ToolbarButton(ClearHistory) { text = "Clear" });
            rootVisualElement.Add(toolbar);

            var columns = new Columns();
            columns.Add(new Column
            {
                name = "time", title = "Time", width = 100, minWidth = 70,
                makeCell = () => new Label { style = { marginLeft = 4, unityTextAlign = TextAnchor.MiddleLeft } },
                bindCell = (e, i) => ((Label)e).text = _records[i].WallClock.ToString("HH:mm:ss.fff")
            });
            columns.Add(new Column
            {
                name = "type", title = "Type", width = 60, minWidth = 50,
                makeCell = () => new Label { style = { marginLeft = 4, unityTextAlign = TextAnchor.MiddleLeft } },
                bindCell = (e, i) =>
                {
                    var record = _records[i];
                    var label = (Label)e;
                    var isClear = record.Kind == EventRecordKind.Clear;
                    label.text = isClear ? "Clear" : "Raise";
                    label.style.color = isClear
                        ? new Color(0.90f, 0.49f, 0.13f) // orange
                        : StyleKeyword.Null;
                }
            });
            columns.Add(new Column
            {
                name = "payload", title = "Payload", width = 180, minWidth = 100, stretchable = true,
                makeCell = () => new Label { style = { marginLeft = 4, unityTextAlign = TextAnchor.MiddleLeft, overflow = Overflow.Hidden, textOverflow = TextOverflow.Ellipsis, whiteSpace = WhiteSpace.NoWrap } },
                bindCell = (e, i) =>
                {
                    var record = _records[i];
                    ((Label)e).text = record.Kind == EventRecordKind.Clear
                        ? "—"
                        : (string.IsNullOrEmpty(record.PayloadString) ? "—" : record.PayloadString);
                }
            });
            columns.Add(new Column
            {
                name = "caller", title = "Caller", width = 220, minWidth = 120, stretchable = true,
                makeCell = () => new Label { style = { marginLeft = 4, unityTextAlign = TextAnchor.MiddleLeft, overflow = Overflow.Hidden, textOverflow = TextOverflow.Ellipsis, whiteSpace = WhiteSpace.NoWrap } },
                bindCell = (e, i) => ((Label)e).text = FormatCaller(_records[i])
            });

            _listView = new MultiColumnListView(columns)
            {
                selectionType = SelectionType.Single,
                showBoundCollectionSize = false,
                style = { flexGrow = 1 }
            };
            _listView.itemsSource = _records;
#if UNITY_2022_2_OR_NEWER
            _listView.itemsChosen += OnItemsChosen;
#else
            _listView.onItemsChosen += OnItemsChosen;
#endif
            rootVisualElement.Add(_listView);

            _emptyLabel = new Label("No events captured yet.")
            {
                style = { marginTop = 8, marginLeft = 8, opacity = 0.6f }
            };
            rootVisualElement.Add(_emptyLabel);
        }

        private void OnItemsChosen(IEnumerable<object> chosen)
        {
            foreach (var obj in chosen)
            {
                if (obj is EventRaiseRecord record) JumpToCaller(record);
                break;
            }
        }

        private static void JumpToCaller(EventRaiseRecord record)
        {
            if (!record.HasCallerSource) return;

            // Try to resolve a project-relative asset path for the source file.
            var path = record.CallerFile.Replace('\\', '/');
            var assetsIndex = path.IndexOf("/Assets/", StringComparison.OrdinalIgnoreCase);
            if (assetsIndex >= 0) path = path.Substring(assetsIndex + 1);

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null)
            {
                AssetDatabase.OpenAsset(script, record.CallerLine);
                return;
            }

            // Absolute-path fallback via Unity's internal opener.
            InternalEditorUtilityOpen(record.CallerFile, record.CallerLine);
        }

        private static void InternalEditorUtilityOpen(string file, int line)
        {
            try
            {
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(file, line);
            }
            catch
            {
                Debug.Log($"[Event Monitor] Could not open {file}:{line}.");
            }
        }

        private void OnEventRaised(EventRaiseRecord record)
        {
            if (!LicenseGate.Current.IsEntitled) return;
            if (_row == null || record.InterfaceType != _row.InterfaceType) return;
            Refresh();
        }

        private void Refresh()
        {
            _records.Clear();
            if (_row != null)
            {
                // Newest first.
                for (var i = _row.History.Count - 1; i >= 0; i--)
                    _records.Add(_row.History[i]);
            }

            _listView?.RefreshItems();
            if (_emptyLabel != null)
                _emptyLabel.style.display = _records.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ClearHistory()
        {
            _row?.History.Clear();
            Refresh();
        }

        private static string FormatCaller(EventRaiseRecord record)
        {
            // Clear records without a traceable caller carry a fixed reason
            // (e.g. "gameobject destroyed" / "scene loaded").
            if (record.Kind == EventRecordKind.Clear && !string.IsNullOrEmpty(record.ClearReason))
                return record.ClearReason;

            if (!string.IsNullOrEmpty(record.CallerType))
            {
                var typeShort = record.CallerType;
                var dot = typeShort.LastIndexOf('.');
                if (dot >= 0 && dot < typeShort.Length - 1) typeShort = typeShort.Substring(dot + 1);

                var head = $"{typeShort}.{record.CallerMethod}";
                return record.CallerLine > 0 ? $"{head} :{record.CallerLine}" : head;
            }

            return record.HasCallerSource ? $"{record.CallerFile}:{record.CallerLine}" : "—";
        }
    }
}
