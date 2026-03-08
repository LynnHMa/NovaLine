using NovaLine.Element;
using NovaLine.Editor.Graph.View;
using System;
using System.Collections.Generic;
using static NovaGraphWindow;
using UnityEngine;

namespace NovaLine.Editor.Graph.Data
{
    [Serializable]
    public class FlowchartGraphViewData : GraphViewNodeData<Flowchart>
    {
        //Node节点信息
        public List<NodeGraphViewData> nodeGraphViewDatas { get; set; } = new ();

        //连接Node节点的连线信息
        public List<NodeEdgeGraphViewData> nodeEdgeGraphViewData { get; set; } = new();
        public FlowchartGraphViewData()
        {
            guid = Guid.NewGuid().ToString();
            linkedElement = new Flowchart("New Flowchart");
        }
        public FlowchartGraphViewData(List<OpenedNovaGraphView> openeds)
        {
            try
            {
                updateChildData(openeds,true);
            }
            catch(Exception e)
            {
                Debug.LogError("Error in storing flowchart data!" + e.Message);
            }
        }
        public void updateChildData(List<OpenedNovaGraphView> openeds,bool isInit = false)
        {
            try
            {
                if (openeds == null || openeds.Count == 0) return;
                var root = openeds[openeds.Count - 1];
                linkedElement = (Flowchart)root.graphView.root;
                startGraphNodeGuid = linkedElement.firstNode?.guid;

                var flowchartGraphView = (FlowchartGraphView)root.graphView;
                var editingNodeGraphView = openeds.Count > 1 ? (NodeGraphView)openeds[openeds.Count - 2]?.graphView : null;
                if (flowchartGraphView == null) return;
                foreach (var graphNode in flowchartGraphView.graphNodes)
                {
                    if (graphNode == null || graphNode.linkedElement is not Element.Node node || node == null)
                    {
                        continue;
                    }

                    NodeGraphViewData newNodeData = null;

                    if (!isInit)
                    {
                        if (editingNodeGraphView != null && editingNodeGraphView.root.guid == node.guid)
                        {
                            newNodeData = new NodeGraphViewData(editingNodeGraphView, graphNode.pos);
                        }
                        else
                        {
                            newNodeData = new NodeGraphViewData(node, graphNode.pos);
                        }
                    }
                    else
                    {
                        newNodeData = new NodeGraphViewData(node, graphNode.pos);

                    }

                    nodeEdgeGraphViewData.Clear();
                    foreach (var nodeSwitcher in node.nextNodes)
                    {
                        //Debug.Log("Saved a edge");
                        nodeEdgeGraphViewData.Add(new NodeEdgeGraphViewData(nodeSwitcher));
                    }

                    if (newNodeData != null) updateSingleNodeData(newNodeData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error in storing flowchart data!" + e.Message);
            }
        }
        public override void draw(INovaGraphView graphView)
        {
            //绘制Node节点
            for (int i = nodeGraphViewDatas.Count - 1; i >= 0; i--)
            {
                var nodeData = nodeGraphViewDatas[i];
                nodeData.draw(graphView);
            }

            //绘制Node节点的连线
            for (int i = nodeEdgeGraphViewData.Count - 1; i >= 0; i--)
            {
                var nodeSwitcherData = nodeEdgeGraphViewData[i];
                nodeSwitcherData.draw(this,graphView);
            }

            graphView.updateAllGraphElements();
        }

        private void updateSingleNodeData(NodeGraphViewData newData)
        {
            for (var i = 0; i < nodeGraphViewDatas.Count; i++)
            {
                var nodeGraphViewData = nodeGraphViewDatas[i];
                if (nodeGraphViewData.guid.Equals(newData.guid))
                {
                    nodeGraphViewDatas[i] = newData;
                    return;
                }
            }
            nodeGraphViewDatas.Add(newData);
        }
    }
    [CreateAssetMenu]
    public class FlowchartGraphViewDataAsset : ScriptableObject
    {
        public FlowchartGraphViewData data;

        public static FlowchartGraphViewDataAsset CreateInstance(FlowchartGraphViewData data = null)
        {
            var result = CreateInstance<FlowchartGraphViewDataAsset>();
            result.data = data == null ? new FlowchartGraphViewData() : data;
            return result;
        }
    }
}
