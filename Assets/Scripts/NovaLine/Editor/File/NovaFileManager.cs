using NovaLine.Element;
using NovaLine.Editor.Graph.Data;
using NovaLine.Editor.Graph.View;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using static NovaGraphWindow;

namespace NovaLine.Editor.File
{
    public class NovaFileManager
    {
        private static string currentPath;

        [OnOpenAsset]
        public static bool loadGraphWindowData(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            currentPath = AssetDatabase.GetAssetOrScenePath(obj);
            if (obj is FlowchartGraphViewData flowchartData)
            {
                getMainWindowInstance().openedGraphViews.Clear();

                var currentFlowchart = flowchartData.to();
                Debug.Log($"Loaded {currentFlowchart.nodes.Count} nodes , {flowchartData.nodeEdgeGraphViewData.Count} edges!");
                flowchartData.name = obj.name;
                loadFlowchartInWindow(flowchartData,new FlowchartGraphView(currentFlowchart));
                return true;
            }
            return false;
        }
        public static FlowchartGraphViewData createAndLoadNewFlowchartDataFile()
        {
            var data = ScriptableObject.CreateInstance<FlowchartGraphViewData>();

            currentPath = EditorUtility.SaveFilePanelInProject(
                "Save New Flowchart",
                "New Flowchart",
                "asset",
                "Save New Flowchart"
            );

            if (string.IsNullOrEmpty(currentPath)) return null;

            AssetDatabase.CreateAsset(data, currentPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return data;
        }
        [Shortcut("NovaLine/Save", typeof(NovaGraphWindow),KeyCode.S, ShortcutModifiers.Action)]
        public static void saveGraphWindowData()
        {
            EditorApplication.delayCall += () =>
            {
                var opened = getMainWindowInstance()?.currentOpenedGraphView;
                var rootData = getUpdatedFlowchartGraphViewData(opened?.graphView?.root);

                if (rootData == null) return;

                if (string.IsNullOrEmpty(currentPath))
                {
                    currentPath = EditorUtility.SaveFilePanelInProject("Save Flowchart", rootData?.name, "asset", "Save Flowchart");
                }

                Debug.Log("Successfully Saved!");
                AssetDatabase.CreateAsset(rootData, currentPath);
                EditorUtility.SetDirty(rootData);
                AssetDatabase.SaveAssets();
            };
        }
        private static FlowchartGraphViewData getUpdatedFlowchartGraphViewData(INovaElement newRoot)
        {
            var openeds = getMainWindowInstance()?.openedGraphViews;
            if (openeds?.Count == 0)
            {
                return null;
            }

            var root = getMainWindowInstance()?.rootOpenedGraphView;
            var current = getMainWindowInstance()?.currentOpenedGraphView;
            var result = (FlowchartGraphViewData)root?.linkedData;

            var iOpened = root;
            if (openeds.Count >= 2)
            {
                for (var i = openeds.Count - 2; i >= 0; i--)
                {
                    var opened = openeds?[i];
                    if (opened.linkedData is NodeGraphViewData nodeGraphViewData)
                    {
                        ((FlowchartGraphViewData)iOpened.linkedData)?.nodeGraphViewDatas?.ForEach((nodeData) =>
                        {
                            if (nodeData.guid.Equals(nodeGraphViewData.guid))
                            {
                                nodeData = (NodeGraphViewData)getUpdatedChildData(opened.linkedData, newRoot);
                                iOpened = opened;
                            }
                        });
                    }
                }
            }

            result = new FlowchartGraphViewData((FlowchartGraphView)root?.graphView);
            root.linkedData = result;
            return result;
        }
        private static IGraphViewNodeData getUpdatedChildData(IGraphViewNodeData old, INovaElement newRoot)
        {
            var opened = getMainWindowInstance()?.currentOpenedGraphView;
            if (newRoot is Node node)
            {
                return new NodeGraphViewData((NodeGraphView)opened.graphView, old.pos);
            }
            else return null;
        }

    }
}
