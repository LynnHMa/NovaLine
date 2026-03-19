using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using static NovaWindow;
using System;
using NovaLine.Editor.Window.Context;

namespace NovaLine.Editor.File
{
    public class EditorFileManager
    {
        private static readonly string CURRENT_PATH_SESSION_PATH_KEY = "NOVA_CURRENT_PATH";
        private static readonly string CURRENT_CONTEXT_GUID_SESSION_PATH_KEY = "NOVA_CURRENT_CONTEXT";
        private static readonly string CURRENT_CONTEXT_TYPE_SESSION_PATH_KEY = "NOVA_CURRENT_CONTEXT_TYPE";
        private static string CurrentPath
        {
            get => SessionState.GetString(CURRENT_PATH_SESSION_PATH_KEY, string.Empty);
            set => SessionState.SetString(CURRENT_PATH_SESSION_PATH_KEY, value);
        }
        public static string CurrentContextGuid
        {
            get => SessionState.GetString(CURRENT_CONTEXT_GUID_SESSION_PATH_KEY, string.Empty);
            set => SessionState.SetString(CURRENT_CONTEXT_GUID_SESSION_PATH_KEY, value);
        }
        public static ContextType CurrentContextType
        {
            get => (ContextType) Enum.Parse(typeof(ContextType), SessionState.GetString(CURRENT_CONTEXT_TYPE_SESSION_PATH_KEY, "NONE"));
            set => SessionState.SetString(CURRENT_CONTEXT_TYPE_SESSION_PATH_KEY, value.ToString());
        }

        private static FlowchartDataAsset _currentAsset;
        private static FlowchartDataAsset currentAsset
        {
            get
            {
                if (_currentAsset == null && !string.IsNullOrEmpty(CurrentPath))
                    _currentAsset = AssetDatabase.LoadAssetAtPath<FlowchartDataAsset>(CurrentPath);
                return _currentAsset;
            }
            set => _currentAsset = value;
        }

        //Fuck u Unity!
        //ScriptableObject is depending on runtime assembly,but it's unconvenient to move all of data script to that assembly.
        //Just force reloading domain after Unity lanughing.....Sorry....I dislike this fucking engine...
        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (!SessionState.GetBool("Initialized", false))
            {
                SessionState.SetBool("Initialized", true);

                EditorApplication.delayCall += () =>
                {
                    EditorUtility.RequestScriptReload();
                };
            }
        }

        [OnOpenAsset]
        public static bool LoadFlowchartDataAsset(int instanceID, int line)
        {
            var path = AssetDatabase.GetAssetPath(instanceID);
            if (string.IsNullOrEmpty(path))
                return false;

            var asset = AssetDatabase.LoadAssetAtPath<FlowchartDataAsset>(path);
            if (asset == null)
            {
                Debug.LogError($"Failed to load asset at path: {path}");
                return false;
            }

            if (asset.data == null)
            {
                Debug.LogError("FlowchartDataAsset.data is null");
                return false;
            }

            currentAsset = asset;
            CurrentPath = path;

            CreateGraphWindow();

            LoadContextInWindow(new FlowchartContext(asset.data));
            return true;
        }

        [Shortcut("NovaLine/Save", typeof(NovaWindow), KeyCode.S, ShortcutModifiers.Action)]
        public static void SaveGraphWindowData()
        {
            var window = GetMainWindowInstance();

            if (window == null)
                return;

            var flowchartContext = window.registeredFlowchartContext;
            var currentContext = window.currentGraphViewContext;

            if (flowchartContext == null || flowchartContext.linkedData == null || currentContext == null || currentContext.linkedData == null)
                return;

            if (string.IsNullOrEmpty(CurrentPath))
            {
                CurrentPath = EditorUtility.SaveFilePanelInProject(
                    "Save Flowchart",
                    flowchartContext.linkedData.name,
                    "asset",
                    "Save Flowchart"
                );

                if (string.IsNullOrEmpty(CurrentPath))
                    return;
            }

            if (!CurrentPath.EndsWith(".asset"))
            {
                Debug.LogError("Invalid save path! Only .asset allowed: " + CurrentPath);
                return;
            }

            if (!currentContext.guid.Equals(flowchartContext.guid)) currentContext.save();
            flowchartContext.save();

            if (currentAsset == null)
            {
                currentAsset = AssetDatabase.LoadAssetAtPath<FlowchartDataAsset>(CurrentPath);

                if (currentAsset == null)
                {
                    currentAsset = FlowchartDataAsset.CreateInstance(flowchartContext.linkedData);
                    AssetDatabase.CreateAsset(currentAsset, CurrentPath);
                }
            }

            currentAsset.data = flowchartContext.linkedData;

            EditorUtility.SetDirty(currentAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RestoreAfterDomainReload()
        {
            var path = CurrentPath;
            if (string.IsNullOrEmpty(path)) return;
            var asset = currentAsset;
            if (asset == null) return;

            var flowchartData = asset.data;
            flowchartData.linkedElement.name = asset.name;
            var flowchartContext = new FlowchartContext(flowchartData);

            //Re-register all contexts
            var storedCurrentContextGuid = CurrentContextGuid;
            var storedCurrentContextType = CurrentContextType;
            LoadContextInWindow(flowchartContext);

            if (!flowchartContext.guid.Equals(storedCurrentContextGuid))
            {
                var toOpenContext = GetContext(storedCurrentContextGuid, storedCurrentContextType);
                if (toOpenContext == null) return;
                LoadContextInWindow(toOpenContext);
            }
        }

        public static FlowchartDataAsset CreateNewFlowchartAsset()
        {
            try
            {
                var dataAsset = FlowchartDataAsset.CreateInstance();

                var path = EditorUtility.SaveFilePanelInProject(
                    "Save New Flowchart",
                    "New Flowchart",
                    "asset",
                    "Save New Flowchart"
                );

                if (string.IsNullOrEmpty(path))
                    return null;

                AssetDatabase.CreateAsset(dataAsset, path);
                AssetDatabase.SaveAssets();

                currentAsset = dataAsset;
                CurrentPath = path;

                return dataAsset;
            }
            catch (Exception e)
            {
                Debug.LogError("Error in creating new flowchart data! " + e);
                return null;
            }
        }
    }
}