using NovaLine.Element;
using NovaLine.Editor.Graph.Data;
using NovaLine.Editor.Graph.View;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using static NovaGraphWindow;
using System;

namespace NovaLine.Editor.File
{
    public class NovaFileManager
    {
        private static string currentPath;

        [OnOpenAsset]
        public static bool loadGraphWindowData(int instanceID, int line)
        {
            try
            {
                var obj = EditorUtility.InstanceIDToObject(instanceID);
                currentPath = AssetDatabase.GetAssetOrScenePath(obj);
                if (obj is FlowchartGraphViewDataAsset flowchartDataAsset)
                {
                    createGraphWindow();
                    getMainWindowInstance().openedGraphViews.Clear();

                    var flowchartData = flowchartDataAsset.data;
                    var currentFlowchart = flowchartData.linkedElement;
                    Debug.Log($"Loaded {currentFlowchart.nodes.Count} nodes , {flowchartData.nodeEdgeGraphViewData.Count} edges!");
                    flowchartData.name = obj.name;
                    EditorApplication.delayCall += () =>
                    {
                        loadFlowchartInWindow(flowchartData, new FlowchartGraphView(currentFlowchart));
                    };
                    return true;
                }
                return false;
            }
            catch(Exception e)
            {
                Debug.LogError("Error in loading flowchart data! " + e.Message);
                return false;
            }
        }
        public static FlowchartGraphViewDataAsset createAndLoadNewFlowchartDataFile()
        {
            try
            {
                var dataAsset = FlowchartGraphViewDataAsset.CreateInstance();

                currentPath = EditorUtility.SaveFilePanelInProject(
                    "Save New Flowchart",
                    "New Flowchart",
                    "asset",
                    "Save New Flowchart"
                );

                if (string.IsNullOrEmpty(currentPath)) return null;

                AssetDatabase.CreateAsset(dataAsset, currentPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return dataAsset;
            }
            catch (Exception e)
            {
                Debug.LogError("Error in creating new flowchart data! " + e.Message);
                return null;
            }  
        }
        [Shortcut("NovaLine/Save", typeof(NovaGraphWindow),KeyCode.S, ShortcutModifiers.Action)]
        public static void saveGraphWindowData()
        {
            try
            {
                var root = getMainWindowInstance().rootOpenedGraphView;
                var opened = getMainWindowInstance().currentOpenedGraphView;
                var openeds = getMainWindowInstance().openedGraphViews;
                if (root == null || root.linkedData == null || root.graphView == null || opened == null || openeds == null || openeds.Count == 0)
                {
                    return;
                }

                //Save child to root
                if (opened.linkedData is NodeGraphViewData)
                {
                    opened.linkedData = new NodeGraphViewData((NodeGraphView)opened?.graphView, opened.linkedData.pos);
                }

                // Save root asset
                if (root.linkedData is FlowchartGraphViewData flowchartGraphViewData)
                {
                    flowchartGraphViewData.updateChildData(openeds);

                    if (string.IsNullOrEmpty(currentPath))
                    {
                        currentPath = EditorUtility.SaveFilePanelInProject("Save Flowchart", root.linkedData?.name, "asset", "Save Flowchart");
                    }

                    var toSave = FlowchartGraphViewDataAsset.CreateInstance((FlowchartGraphViewData)root.linkedData);

                    if (toSave == null) return;

                    //Debug.Log("Successfully Saved!");

                    AssetDatabase.CreateAsset(toSave, currentPath);
                    EditorUtility.SetDirty(toSave);
                    AssetDatabase.SaveAssets();
                }

            }
            catch (Exception e)
            {
                Debug.LogError("Error in saving flowchart data! " + e.Message);
            }
        }
    }
}
