using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameCreator.Editor.Installs
{
    public class DemoBrowserWindow : EditorWindow
    {
        private const string MENU_ITEM = "Game Creator/Demo Browser...";
        private const string InstallsPath = "Assets/Plugins/GameCreator/Installs";

        private const float HeaderHeight = 74f;
        private const float CardWidth = 254f;
        private const float CardHeight = 218f;
        private const float CardGap = 12f;
        private const float PreviewHeight = 88f;

        private static readonly Color Background = new Color(0.12f, 0.13f, 0.16f);
        private static readonly Color HeaderA = new Color(0.18f, 0.31f, 0.46f);
        private static readonly Color HeaderB = new Color(0.43f, 0.20f, 0.44f);
        private static readonly Color EmptyPreview = new Color(0.18f, 0.19f, 0.23f);

        private readonly List<DemoEntry> entries = new List<DemoEntry>();
        private readonly Dictionary<string, Texture2D> previewCache = new Dictionary<string, Texture2D>();

        private Vector2 scroll;
        private string search = string.Empty;
        private string moduleFilter = "All";
        private string[] modules = { "All" };

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle cardTitleStyle;
        private GUIStyle smallStyle;
        private GUIStyle pillStyle;
        private GUIStyle centeredStyle;

        [MenuItem(MENU_ITEM, priority = 32)]
        public static void OpenWindow()
        {
            DemoBrowserWindow window = GetWindow<DemoBrowserWindow>();
            window.titleContent = new GUIContent("GC Demos", EditorGUIUtility.IconContent("SceneAsset Icon").image);
            window.minSize = new Vector2(620f, 430f);
            window.Refresh();
        }

        private void OnEnable()
        {
            this.Refresh();
            EditorApplication.projectChanged += this.Refresh;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= this.Refresh;
        }

        private void OnGUI()
        {
            this.BuildStyles();

            EditorGUI.DrawRect(new Rect(0f, 0f, this.position.width, this.position.height), Background);
            this.DrawHeader();
            this.DrawToolbar();
            this.DrawEntries();

            if (AssetPreview.IsLoadingAssetPreviews())
            {
                this.Repaint();
            }
        }

        private void DrawHeader()
        {
            Rect header = new Rect(0f, 0f, this.position.width, HeaderHeight);
            EditorGUI.DrawRect(header, HeaderA);
            EditorGUI.DrawRect(new Rect(this.position.width * 0.55f, 0f, this.position.width * 0.45f, HeaderHeight), HeaderB);

            Rect title = new Rect(18f, 12f, this.position.width - 36f, 28f);
            GUI.Label(title, "Game Creator Demos", this.titleStyle);

            string summary = this.entries.Count == 1 ? "1 installed demo pack" : $"{this.entries.Count} installed demo packs";
            Rect subtitle = new Rect(20f, 42f, this.position.width - 40f, 22f);
            GUI.Label(subtitle, $"{summary} found in Installs", this.subtitleStyle);
        }

        private void DrawToolbar()
        {
            GUILayout.Space(HeaderHeight + 10f);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(72f)))
            {
                this.Refresh();
            }

            GUILayout.Space(8f);
            GUILayout.Label("Module", GUILayout.Width(48f));

            int moduleIndex = Mathf.Max(0, Array.IndexOf(this.modules, this.moduleFilter));
            int nextModuleIndex = EditorGUILayout.Popup(moduleIndex, this.modules, EditorStyles.toolbarPopup, GUILayout.Width(170f));
            if (nextModuleIndex != moduleIndex)
            {
                this.moduleFilter = this.modules[nextModuleIndex];
            }

            GUILayout.FlexibleSpace();
            this.search = GUILayout.TextField(this.search, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField, GUILayout.Width(230f));

            if (GUILayout.Button(GUIContent.none, GUI.skin.FindStyle("ToolbarSeachCancelButton") ?? EditorStyles.toolbarButton, GUILayout.Width(22f)))
            {
                this.search = string.Empty;
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawEntries()
        {
            List<DemoEntry> visible = this.GetVisibleEntries();

            if (visible.Count == 0)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("No demo packs match the current filters.", this.centeredStyle);
                GUILayout.FlexibleSpace();
                return;
            }

            this.scroll = GUILayout.BeginScrollView(this.scroll);
            GUILayout.Space(12f);

            float availableWidth = Mathf.Max(1f, this.position.width - 24f);
            int columns = Mathf.Max(1, Mathf.FloorToInt((availableWidth + CardGap) / (CardWidth + CardGap)));

            for (int i = 0; i < visible.Count; i += columns)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);

                for (int column = 0; column < columns; ++column)
                {
                    int index = i + column;
                    if (index < visible.Count)
                    {
                        Rect rect = GUILayoutUtility.GetRect(CardWidth, CardHeight, GUILayout.Width(CardWidth), GUILayout.Height(CardHeight));
                        this.DrawCard(rect, visible[index], index);
                    }
                    else
                    {
                        GUILayout.Space(CardWidth);
                    }

                    GUILayout.Space(CardGap);
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(CardGap);
            }

            GUILayout.EndScrollView();
        }

        private void DrawCard(Rect rect, DemoEntry entry, int index)
        {
            Color accent = Color.HSVToRGB((index * 0.097f) % 1f, 0.55f, 0.85f);
            Color card = new Color(0.18f, 0.19f, 0.23f);
            Color cardTop = new Color(accent.r * 0.32f, accent.g * 0.32f, accent.b * 0.32f, 1f);

            EditorGUI.DrawRect(rect, card);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 5f, rect.height), accent);
            EditorGUI.DrawRect(new Rect(rect.x + 5f, rect.y, rect.width - 5f, 40f), cardTop);

            Rect titleRect = new Rect(rect.x + 16f, rect.y + 9f, rect.width - 28f, 20f);
            GUI.Label(titleRect, entry.DisplayName, this.cardTitleStyle);

            Rect previewRect = new Rect(rect.x + 14f, rect.y + 50f, rect.width - 28f, PreviewHeight);
            EditorGUI.DrawRect(previewRect, EmptyPreview);

            Texture2D preview = this.GetPreview(entry);
            if (preview != null)
            {
                GUI.DrawTexture(previewRect, preview, ScaleMode.ScaleAndCrop);
            }
            else
            {
                GUI.Label(previewRect, "Preview unavailable", this.centeredStyle);
            }

            Rect pillRect = new Rect(rect.x + 16f, rect.y + 147f, rect.width - 32f, 18f);
            GUI.Label(pillRect, entry.Module, this.pillStyle);

            Rect infoRect = new Rect(rect.x + 16f, rect.y + 168f, rect.width - 32f, 20f);
            GUI.Label(infoRect, entry.Summary, this.smallStyle);

            Rect buttonA = new Rect(rect.x + 14f, rect.y + rect.height - 34f, 70f, 22f);
            Rect buttonB = new Rect(rect.x + 90f, rect.y + rect.height - 34f, 78f, 22f);
            Rect buttonC = new Rect(rect.x + 174f, rect.y + rect.height - 34f, 66f, 22f);

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(entry.FirstScenePath)))
            {
                if (GUI.Button(buttonA, "Scene"))
                {
                    this.OpenScene(entry.FirstScenePath);
                }
            }

            if (GUI.Button(buttonB, "Folder"))
            {
                this.Ping(entry.Path);
            }

            if (GUI.Button(buttonC, "Select"))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(entry.Path);
            }
        }

        private void Refresh()
        {
            this.entries.Clear();
            this.previewCache.Clear();

            if (!AssetDatabase.IsValidFolder(InstallsPath))
            {
                this.modules = new[] { "All" };
                this.moduleFilter = "All";
                this.Repaint();
                return;
            }

            HashSet<string> moduleSet = new HashSet<string> { "All" };
            string[] folders = AssetDatabase.GetSubFolders(InstallsPath);

            foreach (string folder in folders)
            {
                DemoEntry entry = DemoEntry.Create(folder);
                this.entries.Add(entry);
                moduleSet.Add(entry.Module);
            }

            this.entries.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

            List<string> moduleList = new List<string>(moduleSet);
            moduleList.Sort((a, b) => a == "All" ? -1 : b == "All" ? 1 : string.Compare(a, b, StringComparison.OrdinalIgnoreCase));
            this.modules = moduleList.ToArray();

            if (Array.IndexOf(this.modules, this.moduleFilter) < 0)
            {
                this.moduleFilter = "All";
            }

            this.Repaint();
        }

        private List<DemoEntry> GetVisibleEntries()
        {
            string query = (this.search ?? string.Empty).Trim();
            List<DemoEntry> visible = new List<DemoEntry>();

            foreach (DemoEntry entry in this.entries)
            {
                if (this.moduleFilter != "All" && entry.Module != this.moduleFilter) continue;

                if (!string.IsNullOrEmpty(query) &&
                    entry.DisplayName.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0 &&
                    entry.FolderName.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0 &&
                    entry.Module.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                visible.Add(entry);
            }

            return visible;
        }

        private Texture2D GetPreview(DemoEntry entry)
        {
            if (this.previewCache.TryGetValue(entry.Path, out Texture2D cached))
            {
                return cached;
            }

            if (string.IsNullOrEmpty(entry.PreviewAssetPath))
            {
                this.previewCache[entry.Path] = null;
                return null;
            }

            Object previewAsset = AssetDatabase.LoadAssetAtPath<Object>(entry.PreviewAssetPath);
            Texture2D preview = previewAsset != null ? AssetPreview.GetAssetPreview(previewAsset) : null;
            if (preview == null && previewAsset != null)
            {
                preview = AssetPreview.GetMiniThumbnail(previewAsset);
            }

            if (preview != null)
            {
                this.previewCache[entry.Path] = preview;
            }

            return preview;
        }

        private void OpenScene(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath)) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorSceneManager.OpenScene(scenePath);
        }

        private void Ping(string assetPath)
        {
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            EditorGUIUtility.PingObject(folder);
            Selection.activeObject = folder;
        }

        private void BuildStyles()
        {
            if (this.titleStyle != null) return;

            this.titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                normal = { textColor = Color.white }
            };

            this.subtitleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(1f, 1f, 1f, 0.78f) }
            };

            this.cardTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                clipping = TextClipping.Ellipsis,
                normal = { textColor = Color.white }
            };

            this.smallStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                clipping = TextClipping.Ellipsis,
                normal = { textColor = new Color(0.86f, 0.88f, 0.93f) }
            };

            this.pillStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Ellipsis,
                normal = { textColor = new Color(0.70f, 0.88f, 1f) }
            };

            this.centeredStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.78f, 0.80f, 0.86f) }
            };
        }

        private class DemoEntry
        {
            public string Path { get; private set; }
            public string FolderName { get; private set; }
            public string DisplayName { get; private set; }
            public string Module { get; private set; }
            public string Version { get; private set; }
            public string Summary { get; private set; }
            public string FirstScenePath { get; private set; }
            public string PreviewAssetPath { get; private set; }

            public static DemoEntry Create(string folder)
            {
                string folderName = System.IO.Path.GetFileName(folder);
                string[] versionParts = folderName.Split('@');
                string packageName = versionParts[0];
                string version = versionParts.Length > 1 ? versionParts[versionParts.Length - 1] : string.Empty;

                int dotIndex = packageName.IndexOf('.');
                string module = dotIndex > 0 ? packageName.Substring(0, dotIndex) : packageName;
                string displayName = packageName.Replace('.', ' ');

                string[] scenes = FindFiles(folder, "*.unity");
                string[] prefabs = FindFiles(folder, "*.prefab");
                string[] textures = FindFiles(folder, "*.png", "*.jpg", "*.jpeg", "*.tga", "*.psd");
                string[] assets = FindFiles(folder, "*.asset");

                string previewAssetPath = FirstExisting(textures, prefabs, assets, scenes);
                string summary = BuildSummary(version, scenes.Length, prefabs.Length, assets.Length);

                return new DemoEntry
                {
                    Path = folder,
                    FolderName = folderName,
                    DisplayName = displayName,
                    Module = module,
                    Version = version,
                    Summary = summary,
                    FirstScenePath = scenes.Length > 0 ? scenes[0] : string.Empty,
                    PreviewAssetPath = previewAssetPath
                };
            }

            private static string BuildSummary(string version, int scenes, int prefabs, int assets)
            {
                List<string> parts = new List<string>();
                if (!string.IsNullOrEmpty(version)) parts.Add("v" + version);
                parts.Add(scenes == 1 ? "1 scene" : $"{scenes} scenes");
                if (prefabs > 0) parts.Add(prefabs == 1 ? "1 prefab" : $"{prefabs} prefabs");
                if (assets > 0) parts.Add(assets == 1 ? "1 asset" : $"{assets} assets");

                return string.Join("  |  ", parts);
            }

            private static string[] FindFiles(string folder, params string[] patterns)
            {
                List<string> results = new List<string>();
                string systemFolder = AssetPathToSystemPath(folder);

                if (!Directory.Exists(systemFolder))
                {
                    return results.ToArray();
                }

                foreach (string pattern in patterns)
                {
                    string[] files = Directory.GetFiles(systemFolder, pattern, SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                        results.Add(SystemPathToAssetPath(file));
                    }
                }

                results.Sort(StringComparer.OrdinalIgnoreCase);
                return results.ToArray();
            }

            private static string FirstExisting(params string[][] groups)
            {
                foreach (string[] group in groups)
                {
                    if (group != null && group.Length > 0) return group[0];
                }

                return string.Empty;
            }

            private static string AssetPathToSystemPath(string assetPath)
            {
                string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
                string combined = System.IO.Path.Combine(projectRoot, assetPath);
                return System.IO.Path.GetFullPath(combined);
            }

            private static string SystemPathToAssetPath(string systemPath)
            {
                string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
                string fullPath = System.IO.Path.GetFullPath(systemPath);
                string relativePath = fullPath.Substring(projectRoot.Length + 1);
                return relativePath.Replace('\\', '/');
            }
        }
    }
}
