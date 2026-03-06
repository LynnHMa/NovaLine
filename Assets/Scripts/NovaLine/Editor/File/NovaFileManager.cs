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
            if (obj is FlowchartGraphViewDataAsset flowchartDataAsset)
            {
                getMainWindowInstance().openedGraphViews.Clear();

                var flowchartData = flowchartDataAsset.data;
                var currentFlowchart = flowchartData.to();
                Debug.Log($"Loaded {currentFlowchart.nodes.Count} nodes , {flowchartData.nodeEdgeGraphViewData.Count} edges!");
                flowchartData.name = obj.name;
                loadFlowchartInWindow(flowchartData,new FlowchartGraphView(currentFlowchart));
                return true;
            }
            return false;
        }
        public static FlowchartGraphViewDataAsset createAndLoadNewFlowchartDataFile()
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
        [Shortcut("NovaLine/Save", typeof(NovaGraphWindow),KeyCode.S, ShortcutModifiers.Action)]
        public static void saveGraphWindowData()
        {
            EditorApplication.delayCall += () =>
            {
                var opened = getMainWindowInstance()?.currentOpenedGraphView;
                var newRootData = getUpdatedFlowchartGraphViewData(opened?.graphView?.root);

                if (newRootData == null) return;

                if (string.IsNullOrEmpty(currentPath))
                {
                    currentPath = EditorUtility.SaveFilePanelInProject("Save Flowchart", newRootData?.name, "asset", "Save Flowchart");
                }

                Debug.Log("Successfully Saved!");
                AssetDatabase.CreateAsset(newRootData, currentPath);
                EditorUtility.SetDirty(newRootData);
                AssetDatabase.SaveAssets();
            };
        }
        private static FlowchartGraphViewDataAsset getUpdatedFlowchartGraphViewData(INovaElement newRoot)
        {
            var openeds = getMainWindowInstance()?.openedGraphViews;
            if (openeds?.Count == 0)
            {
                return null;
            }

            var root = getMainWindowInstance()?.rootOpenedGraphView;
            var resultData = (FlowchartGraphViewData)root?.linkedData;

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

            resultData = new FlowchartGraphViewData(openeds);
            root.linkedData = resultData;
            return FlowchartGraphViewDataAsset.CreateInstance(resultData);
        }
        private static IGraphViewNodeData getUpdatedChildData(IGraphViewNodeData old, INovaElement newRoot)
        {
            var opened = getMainWindowInstance()?.currentOpenedGraphView;
            if (newRoot is Node node)
            {
                var nodeGraphViewData = new NodeGraphViewData((NodeGraphView)opened.graphView, old.pos);
                return nodeGraphViewData;
            }
            else return null;
        }

    }
}
