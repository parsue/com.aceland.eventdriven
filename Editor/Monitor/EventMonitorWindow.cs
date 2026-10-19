using System;
using System.Collections.Generic;
using AceLand.EventDriven.Bus;
using AceLand.EventDriven.Bus.Diagnostics;
using AceLand.EventDriven.Editor.Licensing;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AceLand.EventDriven.Editor.Monitor
{
    /// <summary>
    /// Event Monitor window. Lists every registered event interface in edit mode
    /// (source-generated zero-reflection push) and play mode (with live listener /
    /// cache / last-raise state). Selecting a row opens an inspector with listener
    /// listing, caller / interface tracing and (play mode) cache + history access.
    /// </summary>
    internal sealed class EventMonitorWindow : EditorWindow
    {
        private const double HighlightSeconds = 5d;
        private const string ToolName = "AceLand Event Driven — Event Monitor";

        private enum SortColumn { Interface, Description, Status, LastRaise }

        [MenuItem("Tools/AceLand/EventDriven/Event Monitor", false, 20)]
        public static void Open()
        {
            var window = GetWindow<EventMonitorWindow>();
            window.titleContent = new GUIContent("Event Monitor");
            window.minSize = new Vector2(620, 320);
            window.Show();
        }

        // ---- persistent model (survives list rebuilds) ----
        private readonly Dictionary<Type, EventMonitorRow> _rowsByType = new();
        private readonly List<EventMonitorRow> _allRows = new();
        private readonly List<EventMonitorRow> _filteredRows = new();

        private string _searchText = string.Empty;
        private bool _showInspector = true;
        private bool _showNamespace;
        private SortColumn _sortColumn = SortColumn.Interface;
        private bool _sortAscending = true;
        private EventMonitorRow _selected;
        private bool _playModeDefaultSortApplied;
        private int _inspectorStateHash;

        // ---- UI references ----
        private ToolbarButton _reloadButton;
        private ToolbarToggle _inspectorToggle;
        private ToolbarToggle _namespaceToggle;
        private ToolbarSearchField _searchField;
        private MultiColumnListView _listView;
        private VisualElement _inspectorPane;
        private TwoPaneSplitView _split;
        private Label _modeLabel;

        private double _lastUpdateTime;

        private void OnEnable()
        {
            EventMonitorBridge.EventRaised += OnEventRaised;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            LicenseGate.Current.Changed += OnLicenseChanged;
            LicenseGate.Current.EnsureChecked();
            RebuildRoot();
        }

        private void OnDisable()
        {
            EventMonitorBridge.EventRaised -= OnEventRaised;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            LicenseGate.Current.Changed -= OnLicenseChanged;
        }

        // 依目前授權狀態重建整個視窗:未授權時以 IMGUIContainer 繪製引導頁取代工具內容;
        // 授權時建立正常 UI。授權狀態變更(trial/購買生效)時透過 Changed 重新進入本流程。
        private void RebuildRoot()
        {
            rootVisualElement.Clear();

            if (!LicenseGate.Current.IsEntitled)
            {
                _listView = null;
                _inspectorPane = null;
                _split = null;
                rootVisualElement.Add(new IMGUIContainer(() =>
                    LicenseGate.Current.DrawBlockedPage(ToolName))
                {
                    style = { flexGrow = 1, marginLeft = 6, marginRight = 6 }
                });
                return;
            }

            BuildUi();
            RefreshModel(true);
        }

        // 授權狀態變更後,重建視窗以於 trial/購買生效的即時切換(封鎖頁 ↔ 正常內容)。
        private void OnLicenseChanged()
        {
            LicenseGate.Current.EnsureChecked();
            RebuildRoot();
            Repaint();
        }

        private bool IsPlayMode => EditorApplication.isPlayingOrWillChangePlaymode && Application.isPlaying;

        // ------------------------------------------------------------------ UI
        private void BuildUi()
        {
            var toolbar = new Toolbar();

            _reloadButton = new ToolbarButton(() => RefreshModel(true)) { text = "Reload" };
            toolbar.Add(_reloadButton);

            _modeLabel = new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, marginLeft = 8, marginRight = 8, opacity = 0.7f } };
            toolbar.Add(_modeLabel);

            var spacer = new ToolbarSpacer { style = { flexGrow = 1 } };
            toolbar.Add(spacer);

            _searchField = new ToolbarSearchField { style = { width = 220 } };
            _searchField.RegisterValueChangedCallback(evt =>
            {
                _searchText = evt.newValue ?? string.Empty;
                ApplyFilterAndSort();
            });
            toolbar.Add(_searchField);

            _namespaceToggle = new ToolbarToggle { text = "Namespace", value = _showNamespace };
            _namespaceToggle.tooltip = "Show the full namespace in the Interface column.";
            _namespaceToggle.RegisterValueChangedCallback(evt =>
            {
                _showNamespace = evt.newValue;
                _listView?.RefreshItems();
            });
            toolbar.Add(_namespaceToggle);

            _inspectorToggle = new ToolbarToggle { text = "Inspector", value = _showInspector };
            _inspectorToggle.RegisterValueChangedCallback(evt =>
            {
                _showInspector = evt.newValue;
                UpdateInspectorVisibility();
            });
            toolbar.Add(_inspectorToggle);

            rootVisualElement.Add(toolbar);

            _split = new TwoPaneSplitView(1, 280, TwoPaneSplitViewOrientation.Horizontal)
            {
                style = { flexGrow = 1 }
            };

            _listView = BuildListView();
            var listContainer = new VisualElement { style = { flexGrow = 1, minWidth = 260 } };
            listContainer.Add(_listView);

            _inspectorPane = new ScrollView { style = { minWidth = 220 } };

            _split.Add(listContainer);
            _split.Add(_inspectorPane);
            rootVisualElement.Add(_split);

            UpdateInspectorVisibility();
        }

        private MultiColumnListView BuildListView()
        {
            var columns = new Columns();

            columns.Add(new Column
            {
                name = "interface", title = "Interface", width = 220, minWidth = 120, stretchable = true,
                makeCell = () => NewCellLabel(),
                bindCell = (e, i) => BindInterfaceCell((Label)e, _filteredRows[i])
            });
            columns.Add(new Column
            {
                name = "description", title = "Description", width = 220, minWidth = 100, stretchable = true,
                makeCell = () => NewCellLabel(),
                bindCell = (e, i) => ((Label)e).text = _filteredRows[i].Description ?? string.Empty
            });
            columns.Add(new Column
            {
                name = "status", title = "Status", width = 90, minWidth = 70,
                makeCell = () => NewCellLabel(),
                bindCell = (e, i) => ((Label)e).text = _filteredRows[i].StatusLabel
            });
            columns.Add(new Column
            {
                name = "lastRaise", title = "Last Raise", width = 110, minWidth = 80,
                makeCell = () => NewCellLabel(),
                bindCell = (e, i) => ((Label)e).text = FormatLastRaise(_filteredRows[i])
            });

            var view = new MultiColumnListView(columns)
            {
                selectionType = SelectionType.Single,
                showBoundCollectionSize = false,
                style = { flexGrow = 1 }
            };
            view.itemsSource = _filteredRows;

#if UNITY_2022_2_OR_NEWER
            view.selectedIndicesChanged += OnSelectionChanged;
#else
            view.onSelectedIndicesChange += OnSelectionChanged;
#endif

            // Column header click → sorting.
            foreach (var col in columns)
            {
                var captured = col;
                col.makeHeader = () =>
                {
                    var header = new Label(captured.title) { style = { unityTextAlign = TextAnchor.MiddleLeft, flexGrow = 1 } };
                    header.RegisterCallback<PointerDownEvent>(_ => ToggleSort(ColumnToSort(captured.name)));
                    return header;
                };
            }

            return view;
        }

        private static Label NewCellLabel()
        {
            return new Label
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginLeft = 4, marginRight = 4,
                    overflow = Overflow.Hidden,
                    textOverflow = TextOverflow.Ellipsis,
                    whiteSpace = WhiteSpace.NoWrap
                }
            };
        }

        private void BindInterfaceCell(Label label, EventMonitorRow row)
        {
            label.text = FormatInterfaceName(row);
            label.tooltip = row.InterfaceType?.FullName ?? row.InterfaceName;
            ApplyHighlight(label, row);
        }

        private string FormatInterfaceName(EventMonitorRow row)
        {
            // Base is always the simple interface name. DisplayName from the source
            // generator is a fully-qualified name (ToDisplayString), so using it as the
            // base would already include the namespace; we must not rely on it here.
            var simpleName = !string.IsNullOrEmpty(row.InterfaceName)
                ? row.InterfaceName
                : row.InterfaceType?.Name ?? string.Empty;

            if (_showNamespace && row.InterfaceType != null && !string.IsNullOrEmpty(row.InterfaceType.Namespace))
                return row.InterfaceType.Namespace + "." + simpleName;
            return simpleName;
        }

        private static SortColumn ColumnToSort(string name) => name switch
        {
            "description" => SortColumn.Description,
            "status" => SortColumn.Status,
            "lastRaise" => SortColumn.LastRaise,
            _ => SortColumn.Interface
        };

        private void UpdateInspectorVisibility()
        {
            if (_inspectorPane == null) return;
            if (_showInspector)
            {
                if (_inspectorPane.parent == null) _split.Add(_inspectorPane);
            }
            else
            {
                if (_inspectorPane.parent != null) _split.Remove(_inspectorPane);
            }
        }

        // -------------------------------------------------------------- model
        private void RefreshModel(bool rebuildAll)
        {
            var entries = EventBus.EditorGetMonitorEntries();
            var seen = new HashSet<Type>();

            foreach (var entry in entries)
            {
                if (entry.InterfaceType == null) continue;
                seen.Add(entry.InterfaceType);

                if (!_rowsByType.TryGetValue(entry.InterfaceType, out var row))
                {
                    row = new EventMonitorRow(entry.InterfaceType, entry.InterfaceType.Name);
                    _rowsByType[entry.InterfaceType] = row;
                }

                row.DisplayName = entry.DisplayName;
                row.Description = entry.Description;
                row.HasPayload = entry.HasPayload;
                row.PayloadType = entry.PayloadType;
                row.ListenerCount = entry.ListenerCount;
                row.IsCached = entry.IsCached;
            }

            // Drop rows no longer registered.
            if (rebuildAll)
            {
                var stale = new List<Type>();
                foreach (var kvp in _rowsByType)
                    if (!seen.Contains(kvp.Key)) stale.Add(kvp.Key);
                foreach (var t in stale) _rowsByType.Remove(t);
            }

            _allRows.Clear();
            foreach (var kvp in _rowsByType) _allRows.Add(kvp.Value);

            UpdateModeLabel();
            ApplyPlayModeDefaultSort();
            ApplyFilterAndSort();
        }

        private void UpdateModeLabel()
        {
            if (_modeLabel == null) return;
            _modeLabel.text = IsPlayMode
                ? $"Play Mode · {_allRows.Count} events"
                : $"Edit Mode · {_allRows.Count} events";
        }

        private void ApplyPlayModeDefaultSort()
        {
            if (IsPlayMode)
            {
                if (!_playModeDefaultSortApplied)
                {
                    _sortColumn = SortColumn.LastRaise;
                    _sortAscending = false;
                    _playModeDefaultSortApplied = true;
                }
            }
            else
            {
                _playModeDefaultSortApplied = false;
            }
        }

        private void ApplyFilterAndSort()
        {
            _filteredRows.Clear();

            var query = _searchText?.Trim();
            var hasQuery = !string.IsNullOrEmpty(query);

            foreach (var row in _allRows)
            {
                if (hasQuery && !MatchesSearch(row, query)) continue;
                _filteredRows.Add(row);
            }

            _filteredRows.Sort(CompareRows);

            _listView?.RefreshItems();
            ReselectAfterRebuild();
        }

        private static bool MatchesSearch(EventMonitorRow row, string query)
        {
            return (row.InterfaceName?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                || ((row.DisplayName?.IndexOf(query, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
                || ((row.Description?.IndexOf(query, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);
        }

        private int CompareRows(EventMonitorRow a, EventMonitorRow b)
        {
            int cmp;
            switch (_sortColumn)
            {
                case SortColumn.Description:
                    cmp = string.Compare(a.Description, b.Description, StringComparison.OrdinalIgnoreCase);
                    break;
                case SortColumn.Status:
                    cmp = string.Compare(a.StatusLabel, b.StatusLabel, StringComparison.OrdinalIgnoreCase);
                    break;
                case SortColumn.LastRaise:
                    cmp = a.LastRaiseEditorTime.CompareTo(b.LastRaiseEditorTime);
                    break;
                default:
                    cmp = string.Compare(a.InterfaceName, b.InterfaceName, StringComparison.OrdinalIgnoreCase);
                    break;
            }

            if (cmp == 0)
                cmp = string.Compare(a.InterfaceName, b.InterfaceName, StringComparison.OrdinalIgnoreCase);

            return _sortAscending ? cmp : -cmp;
        }

        private void ToggleSort(SortColumn column)
        {
            if (_sortColumn == column) _sortAscending = !_sortAscending;
            else { _sortColumn = column; _sortAscending = true; }
            ApplyFilterAndSort();
        }

        private void ReselectAfterRebuild()
        {
            if (_selected == null) return;
            var index = _filteredRows.IndexOf(_selected);
            if (index >= 0)
            {
                _listView.SetSelectionWithoutNotify(new[] { index });
            }
            else
            {
                _selected = null;
                _inspectorStateHash = 0;
                BuildInspector(null);
            }
        }

        // ------------------------------------------------------------ selection
        private void OnSelectionChanged(IEnumerable<int> indices)
        {
            EventMonitorRow row = null;
            foreach (var i in indices)
            {
                if (i >= 0 && i < _filteredRows.Count) row = _filteredRows[i];
                break;
            }

            _selected = row;
            _inspectorStateHash = ComputeInspectorStateHash(row);
            BuildInspector(row);
        }

        private void BuildInspector(EventMonitorRow row)
        {
            _inspectorPane.Clear();
            if (row == null)
            {
                _inspectorPane.Add(new Label("Select an event to inspect.")
                {
                    style = { marginTop = 8, marginLeft = 8, opacity = 0.6f }
                });
                return;
            }

            _inspectorPane.Add(Header(row.DisplayName ?? row.InterfaceName));

            if (!string.IsNullOrEmpty(row.Description))
                _inspectorPane.Add(InfoLine("Description", row.Description));

            _inspectorPane.Add(InfoLine("Payload", row.HasPayload ? (row.PayloadType?.Name ?? "<payload>") : "None"));
            _inspectorPane.Add(InfoLine("Full Name", row.InterfaceType?.FullName ?? row.InterfaceName));

            // Interface source tracer: ping / open the declaring script.
            var openScript = new Button(() => PingInterfaceScript(row)) { text = "Open Interface Script" };
            openScript.style.marginTop = 4;
            _inspectorPane.Add(openScript);

            if (IsPlayMode)
                BuildPlayModeInspector(row);
            else
                BuildEditModeInspector(row);
        }

        private void BuildEditModeInspector(EventMonitorRow row)
        {
            _inspectorPane.Add(SectionLabel("Listeners (edit mode: none at runtime)"));
            _inspectorPane.Add(new Label("Listener binding is only available in play mode.")
            {
                style = { marginLeft = 8, opacity = 0.6f, whiteSpace = WhiteSpace.Normal }
            });
        }

        private void BuildPlayModeInspector(EventMonitorRow row)
        {
            _inspectorPane.Add(InfoLine("Status", row.StatusLabel));
            _inspectorPane.Add(InfoLine("Raise Count", row.RaiseCount.ToString()));
            _inspectorPane.Add(InfoLine("Last Raise", FormatLastRaiseTime(row)));

            // Cache section.
            var cache = EventBus.EditorGetCacheInfo(row.InterfaceType);
            _inspectorPane.Add(SectionLabel("Cache"));
            if (cache.HasCache)
            {
                _inspectorPane.Add(InfoLine("Binding", cache.Binding.ToString()));
                _inspectorPane.Add(InfoLine("Value", cache.PayloadString));
            }
            else
            {
                _inspectorPane.Add(new Label("No cache recorded.") { style = { marginLeft = 8, opacity = 0.6f } });
            }

            // Listener section with caller/target tracing.
            _inspectorPane.Add(SectionLabel($"Listeners ({row.ListenerCount})"));
            var listeners = EventBus.EditorGetListeners(row.InterfaceType);
            if (listeners.Count == 0)
            {
                _inspectorPane.Add(new Label("No listeners bound.") { style = { marginLeft = 8, opacity = 0.6f } });
            }
            else
            {
                foreach (var l in listeners)
                    _inspectorPane.Add(BuildListenerRow(l));
            }

            // History access.
            var historyButton = new Button(() => EventRaiseHistoryWindow.Open(row)) { text = "Open Event History" };
            historyButton.style.marginTop = 6;
            _inspectorPane.Add(historyButton);
        }

        private VisualElement BuildListenerRow(EventListenerInfo info)
        {
            var container = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, marginLeft = 8, marginTop = 2, alignItems = Align.Center }
            };

            var typeName = info.TargetType?.Name ?? "<static>";
            var label = new Label($"{typeName}.{info.MethodName}")
            {
                style = { flexGrow = 1, whiteSpace = WhiteSpace.NoWrap, overflow = Overflow.Hidden, textOverflow = TextOverflow.Ellipsis }
            };
            container.Add(label);

            if (info.UnityTarget != null)
            {
                // Resolve a hierarchy-pingable object: a Component highlights far better
                // when its owning GameObject is pinged/selected than the component itself.
                var pingTarget = info.UnityTarget is Component component && component != null
                    ? component.gameObject
                    : info.UnityTarget;

                var ping = new Button(() =>
                {
                    if (pingTarget == null) return;
                    Selection.activeObject = pingTarget;
                    EditorGUIUtility.PingObject(pingTarget);
                }) { text = "Ping" };
                ping.style.marginLeft = 4;
                container.Add(ping);
            }

            return container;
        }

        private static void PingInterfaceScript(EventMonitorRow row)
        {
            if (row?.InterfaceType == null) return;
            var type = row.InterfaceType;

            // 1) Fast path: a script whose primary class matches the type (file name == type name).
            foreach (var guid in AssetDatabase.FindAssets($"{type.Name} t:MonoScript"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                {
                    EditorGUIUtility.PingObject(script);
                    Selection.activeObject = script;
                    AssetDatabase.OpenAsset(script);
                    return;
                }
            }

            // 2) Interfaces (and nested types) seldom match the file name, so GetClass()
            //    returns null. Scan script sources for the actual declaration.
            if (TryFindDeclaringScript(type, out var declaring, out var line))
            {
                EditorGUIUtility.PingObject(declaring);
                Selection.activeObject = declaring;
                AssetDatabase.OpenAsset(declaring, line);
                return;
            }

            Debug.Log($"[Event Monitor] Could not locate a MonoScript for {type.FullName}.");
        }

        private static bool TryFindDeclaringScript(Type type, out MonoScript script, out int line)
        {
            script = null;
            line = 0;

            // Match "interface ITestEvent", "class ITestEvent", "struct ITestEvent", "record ITestEvent".
            var pattern = new System.Text.RegularExpressions.Regex(
                $@"\b(?:interface|class|struct|record)\s+{System.Text.RegularExpressions.Regex.Escape(type.Name)}\b");

            foreach (var guid in AssetDatabase.FindAssets("t:MonoScript"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;

                string text;
                try { text = System.IO.File.ReadAllText(path); }
                catch { continue; }

                var match = pattern.Match(text);
                if (!match.Success) continue;

                script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script == null) continue;

                // 1-based line number of the declaration for OpenAsset.
                line = 1;
                for (var i = 0; i < match.Index && i < text.Length; i++)
                    if (text[i] == '\n') line++;
                return true;
            }

            return false;
        }

        // ----------------------------------------------------------- raise hook
        private void OnEventRaised(EventRaiseRecord record)
        {
            if (record.InterfaceType == null) return;
            if (!_rowsByType.TryGetValue(record.InterfaceType, out var row)) return;

            // Clear records are logged in history only; they must not count as raises
            // or trigger the live raise highlight.
            if (record.Kind == EventRecordKind.Raise)
            {
                row.RaiseCount++;
                row.LastRaiseEditorTime = record.EditorTime;
                row.LastRaiseWallClock = record.WallClock;
                row.HighlightRemaining = HighlightSeconds;
            }

            row.History.Add(record);
            if (row.History.Count > 500) row.History.RemoveAt(0);

            // Listener/cache counts may have shifted at raise/clear-time.
            if (EventBus.EditorTryGetMonitorEntry(record.InterfaceType, out var entry))
            {
                row.ListenerCount = entry.ListenerCount;
                row.IsCached = entry.IsCached;
            }
        }

        private void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (!LicenseGate.Current.IsEntitled) return;
            _playModeDefaultSortApplied = false;
            RefreshModel(true);
        }

        // -------------------------------------------------------------- update
        private void Update()
        {
            // 封鎖頁模式下工具 UI 尚未建立(_listView / _inspectorPane 為 null),不進行刷新。
            if (!LicenseGate.Current.IsEntitled) return;

            var now = EditorApplication.timeSinceStartup;

            // Throttle the live refresh to ~5 Hz.
            if (now - _lastUpdateTime < 0.2d) { TickHighlights(now); return; }
            _lastUpdateTime = now;

            if (IsPlayMode)
            {
                RefreshModel(false);
                // Live-refresh the open inspector so re-raises, listener changes and
                // cache invalidation (e.g. a destroyed bound GameObject) reflect
                // without a manual reselect/reload. Only rebuild when the selected
                // row's inspector-relevant state actually changed, so the rebuild
                // does not interrupt button clicks on every tick.
                if (_showInspector && _selected != null && _filteredRows.Contains(_selected))
                {
                    var hash = ComputeInspectorStateHash(_selected);
                    if (hash != _inspectorStateHash)
                    {
                        _inspectorStateHash = hash;
                        BuildInspector(_selected);
                    }
                }
            }

            TickHighlights(now);
        }

        private int ComputeInspectorStateHash(EventMonitorRow row)
        {
            if (row == null) return 0;
            var hash = new HashCode();
            hash.Add(row.RaiseCount);
            hash.Add(row.ListenerCount);
            hash.Add(row.IsCached);
            if (IsPlayMode)
            {
                var cache = EventBus.EditorGetCacheInfo(row.InterfaceType);
                hash.Add(cache.HasCache);
                hash.Add(cache.Binding);
                hash.Add(cache.PayloadString);
            }
            return hash.ToHashCode();
        }

        private void TickHighlights(double now)
        {
            var anyActive = false;
            foreach (var row in _filteredRows)
            {
                if (row.HighlightRemaining <= 0d) continue;
                var elapsed = now - row.LastRaiseEditorTime;
                row.HighlightRemaining = Math.Max(0d, HighlightSeconds - elapsed);
                anyActive = true;
            }

            if (anyActive) _listView?.RefreshItems();
        }

        private void ApplyHighlight(Label label, EventMonitorRow row)
        {
            var container = label.parent ?? label;
            if (row.HighlightRemaining <= 0d)
            {
                container.style.backgroundColor = StyleKeyword.Null;
                return;
            }

            var t = (float)(row.HighlightRemaining / HighlightSeconds); // 1 → 0
            var highlight = new Color(0.90f, 0.49f, 0.13f, 1f); // orange
            var baseColor = new Color(0f, 0f, 0f, 0f);
            container.style.backgroundColor = Color.Lerp(baseColor, highlight, t);
        }

        // --------------------------------------------------------------- helpers
        private static Label Header(string text)
        {
            return new Label(text)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 13,
                    marginTop = 6, marginLeft = 8, marginBottom = 4,
                    whiteSpace = WhiteSpace.Normal
                }
            };
        }

        private static Label SectionLabel(string text)
        {
            return new Label(text)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 8, marginLeft = 8, marginBottom = 2
                }
            };
        }

        private static VisualElement InfoLine(string key, string value)
        {
            var row = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, marginLeft = 8, marginTop = 1 }
            };
            row.Add(new Label(key + ":") { style = { width = 90, opacity = 0.7f } });
            row.Add(new Label(value) { style = { flexGrow = 1, whiteSpace = WhiteSpace.Normal } });
            return row;
        }

        private string FormatLastRaise(EventMonitorRow row)
        {
            if (!row.HasEverRaised) return "—";
            var ago = EditorApplication.timeSinceStartup - row.LastRaiseEditorTime;
            if (ago < 1d) return "just now";
            if (ago < 60d) return $"{ago:0}s ago";
            if (ago < 3600d) return $"{ago / 60d:0}m ago";
            return row.LastRaiseWallClock.ToString("HH:mm:ss");
        }

        private string FormatLastRaiseTime(EventMonitorRow row)
        {
            return row.HasEverRaised ? row.LastRaiseWallClock.ToString("HH:mm:ss") : "—";
        }
    }
}
