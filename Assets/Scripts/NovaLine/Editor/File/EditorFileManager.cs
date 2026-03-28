using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using System;
using NovaLine.Editor.Window;
using NovaLine.Editor.Window.Context;
using static NovaLine.Editor.Window.NovaWindow;
using static NovaLine.Editor.Window.WindowContextRegistry;
using NovaLine.File;
using NovaLine.Element;

namespace NovaLine.Editor.File
{
    public static class EditorFileManager
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

        public static NovaElementType CurrentContextType
        {
            get => (NovaElementType) Enum.Parse(typeof(NovaElementType), SessionState.GetString(CURRENT_CONTEXT_TYPE_SESSION_PATH_KEY, "NONE"));
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
            if (Instance == null)
                return;

            if (RegisteredFlowchartContext == null || RegisteredFlowchartContext.linkedData == null || CurrentGraphViewContext == null || CurrentGraphViewContext.linkedData == null)
                return;

            if (string.IsNullOrEmpty(CurrentPath))
            {
                CurrentPath = EditorUtility.SaveFilePanelInProject(
                    "Save Flowchart",
                    RegisteredFlowchartContext.linkedData.name,
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

            if (!CurrentGraphViewContext.guid.Equals(RegisteredFlowchartContext.guid)) CurrentGraphViewContext.saveData();

            RegisteredFlowchartContext.saveData();

            if (currentAsset == null)
            {
                currentAsset = AssetDatabase.LoadAssetAtPath<FlowchartDataAsset>(CurrentPath);

                if (currentAsset == null)
                {
                    currentAsset = FlowchartDataAsset.CreateInstance(RegisteredFlowchartContext.linkedData);
                    AssetDatabase.CreateAsset(currentAsset, CurrentPath);
                }
            }

            currentAsset.data = RegisteredFlowchartContext.linkedData;

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