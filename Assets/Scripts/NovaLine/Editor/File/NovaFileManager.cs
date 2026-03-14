using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using static NovaWindow;
using System;
using NovaLine.Editor.Graph.Data.NodeGraphView;
using NovaLine.Editor.Window.Context;

namespace NovaLine.Editor.File
{
    public class NovaFileManager
    {
        private static readonly string CURRENT_PATH_SESSION_PATH_KEY = "NOVA_CURRENT_PATH";
        private static readonly string CURRENT_CONTEXT_GUID_SESSION_PATH_KEY = "NOVA_CURRENT_CONTEXT";
        private static string currentPath
        {
            get => SessionState.GetString(CURRENT_PATH_SESSION_PATH_KEY, string.Empty);
            set => SessionState.SetString(CURRENT_PATH_SESSION_PATH_KEY, value);
        }
        public static string CurrentContextGuid
        {
            get => SessionState.GetString(CURRENT_CONTEXT_GUID_SESSION_PATH_KEY, string.Empty);
            set => SessionState.SetString(CURRENT_CONTEXT_GUID_SESSION_PATH_KEY, value);
        }

        private static FlowchartDataAsset _currentAsset;
        private static FlowchartDataAsset currentAsset
        {
            get
            {
                if (_currentAsset == null && !string.IsNullOrEmpty(currentPath))
                    _currentAsset = AssetDatabase.LoadAssetAtPath<FlowchartDataAsset>(currentPath);
                return _currentAsset;
            }
            set => _currentAsset = value;
        }

        [OnOpenAsset]
        public static bool LoadFlowchartDataAsset(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);

            if (obj is FlowchartDataAsset flowchartDataAsset)
            {
                currentAsset = flowchartDataAsset;
                currentPath = AssetDatabase.GetAssetPath(flowchartDataAsset);

                CreateGraphWindow();

                var window = GetMainWindowInstance();

                if (window == null)
                {
                    Debug.LogError("Failed to create new window!");
                    return false;
                }

                var flowchartData = flowchartDataAsset.data;
                var flowchartContext = new FlowchartContext(flowchartData);

                LoadContextInWindow(flowchartContext);

                return true;
            }

            return false;
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

            if (string.IsNullOrEmpty(currentPath))
            {
                currentPath = EditorUtility.SaveFilePanelInProject(
                    "Save Flowchart",
                    flowchartContext.linkedData.name,
                    "asset",
                    "Save Flowchart"
                );

                if (string.IsNullOrEmpty(currentPath))
                    return;
            }

            if (!currentPath.EndsWith(".asset"))
            {
                Debug.LogError("Invalid save path! Only .asset allowed: " + currentPath);
                return;
            }

            if (!currentContext.guid.Equals(flowchartContext.guid)) currentContext.save();
            flowchartContext.save();

            if (currentAsset == null)
            {
                currentAsset = AssetDatabase.LoadAssetAtPath<FlowchartDataAsset>(currentPath);

                if (currentAsset == null)
                {
                    currentAsset = FlowchartDataAsset.CreateInstance(flowchartContext.linkedData);
                    AssetDatabase.CreateAsset(currentAsset, currentPath);
                }
            }

            currentAsset.data = flowchartContext.linkedData;

            EditorUtility.SetDirty(currentAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RestoreAfterDomainReload()
        {
            var path = currentPath;
            if (string.IsNullOrEmpty(path)) return;
            var asset = currentAsset;
            if (asset == null) return;

            var flowchartData = asset.data;
            flowchartData.linkedElement.name = asset.name;
            var flowchartContext = new FlowchartContext(flowchartData);

            //Re-register all contexts
            var storedCurrentContextGuid = CurrentContextGuid;
            LoadContextInWindow(flowchartContext);

            if (!flowchartContext.guid.Equals(storedCurrentContextGuid))
            {
                var toOpenContext = GetContext(storedCurrentContextGuid);
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
                currentPath = path;

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