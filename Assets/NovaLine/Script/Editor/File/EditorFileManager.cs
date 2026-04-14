using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using System;
using NovaLine.Script.Data;
using NovaLine.Script.Editor.Window;
using NovaLine.Script.Editor.Window.Context;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using static NovaLine.Script.Editor.Window.NovaWindow;
using static NovaLine.Script.Editor.Window.ContextRegistry;
using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.File
{
    public static class EditorFileManager
    {
        private const string CURRENT_PATH_SESSION_PATH_KEY = "NOVA_CURRENT_PATH";
        private const string CURRENT_CONTEXT_GUID_SESSION_PATH_KEY = "NOVA_CURRENT_CONTEXT";
        private const string CURRENT_CONTEXT_TYPE_SESSION_PATH_KEY = "NOVA_CURRENT_CONTEXT_TYPE";

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
        private static FlowchartDataAsset CurrentAsset
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
                return false;
            }

            if (asset.data == null)
            {
                Debug.LogError("FlowchartDataAsset.data is null");
                return false;
            }

            CurrentAsset = asset;
            CurrentPath = path;

            CreateGraphWindow();
            LoadContextInWindow(new FlowchartNodeContext(asset.data));

            return true;
        }

        [Shortcut("NovaLine/Save", typeof(NovaWindow), KeyCode.S, ShortcutModifiers.Action)]
        public static void SaveGraphWindowData()
        {
            if (Instance == null)
                return;

            if (RegisteredFlowchartNodeContext?.LinkedData == null || CurrentGraphViewNodeContext?.LinkedData == null)
                return;

            if (string.IsNullOrEmpty(CurrentPath))
            {
                CurrentPath = EditorUtility.SaveFilePanelInProject(
                    "Save Flowchart",
                    RegisteredFlowchartNodeContext.LinkedData.Name,
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

            if (!CurrentGraphViewNodeContext.Guid.Equals(RegisteredFlowchartNodeContext.Guid)) CurrentGraphViewNodeContext.SaveData();

            RegisteredFlowchartNodeContext.SaveData();

            if (CurrentAsset == null)
            {
                CurrentAsset = AssetDatabase.LoadAssetAtPath<FlowchartDataAsset>(CurrentPath);

                if (CurrentAsset == null)
                {
                    CurrentAsset = FlowchartDataAsset.CreateInstance(RegisteredFlowchartNodeContext.LinkedData);
                    AssetDatabase.CreateAsset(CurrentAsset, CurrentPath);
                }
            }

            CurrentAsset.data = RegisteredFlowchartNodeContext.LinkedData;

            EditorUtility.SetDirty(CurrentAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RestoreAfterDomainReload()
        {
            var path = CurrentPath;
            if (string.IsNullOrEmpty(path)) return;
            var asset = CurrentAsset;
            if (asset == null) return;

            var flowchartData = asset.data;
            flowchartData.LinkedElement.name = asset.name;
            var flowchartContext = new FlowchartNodeContext(flowchartData);
            
            //Re-register all contexts
            var storedCurrentContextGuid = CurrentContextGuid;
            var storedCurrentContextType = CurrentContextType;
            LoadContextInWindow(flowchartContext);

            if (!flowchartContext.Guid.Equals(storedCurrentContextGuid))
            {
                var toOpenContext = GetContext(storedCurrentContextGuid, storedCurrentContextType);
                if (toOpenContext is not IGraphViewNodeContext graphViewNodeContext) return;
                LoadContextInWindow(graphViewNodeContext);
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

                CurrentAsset = dataAsset;
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