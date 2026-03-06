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
        public List<NodeGraphViewData> nodeGraphViewDatas { get; set; } = new ();

        public List<NodeEdgeGraphViewData> nodeEdgeGraphViewData { get; set; } = new();

        public FlowchartGraphViewData()
        {
            guid = Guid.NewGuid().ToString();
            name = "New Flowchart";
        }
        public FlowchartGraphViewData(List<OpenedNovaGraphView> openeds)
        {
            if(openeds == null || openeds.Count == 0) return;
            var flowchartGraphView = (FlowchartGraphView)openeds[openeds.Count - 1]?.graphView;
            var nodeGraphView = openeds.Count > 1 ? (NodeGraphView)openeds[openeds.Count - 2]?.graphView : null;
            if (flowchartGraphView == null) return;
            foreach (var graphNode in flowchartGraphView.graphNodes)
            {
                if (graphNode == null || graphNode.pos == null) continue;
                var node = (Element.Node)graphNode.targetObject;
                if (node == null) continue;

                if(nodeGraphView == null)
                {
                    nodeGraphViewDatas?.Add(new NodeGraphViewData(node, graphNode.pos));
                }
                else
                {
                    nodeGraphViewDatas?.Add(new NodeGraphViewData(nodeGraphView, graphNode.pos));
                }

                foreach (var nodeSwitcher in node.nextNodes)
                {
                    nodeEdgeGraphViewData.Add(new NodeEdgeGraphViewData(nodeSwitcher));
                }
            }
        }
        public override Flowchart to()
        {
            var nodes = new List<Element.Node>();

            //导入全部Node到Flowchart
            foreach (var nodeData in nodeGraphViewDatas)
            {
                var node = nodeData.to();
                nodes.Add(node);

                //添加Node的分支
                for (int i = nodeEdgeGraphViewData.Count - 1; i >= 0; i--)
                {
                    var nodeSwitcherData = nodeEdgeGraphViewData[i];
                    var nodeSwitcher = nodeSwitcherData.to(this);
                    if (nodeSwitcher?.outputElement?.guid == node?.guid)
                    {
                        node?.nextNodes?.Add(nodeSwitcher);
                    }
                }
            }
            return new Flowchart(name, describtion, nodes, guid);
        }
        public override void draw(INovaGraphView graphView)
        {
            //绘制Node节点
            foreach(var nodeData in nodeGraphViewDatas)
            {
                nodeData.draw(graphView);
            }

            //绘制Node节点的连线
            for (int i = nodeEdgeGraphViewData.Count - 1; i >= 0; i--)
            {
                var nodeSwitcherData = nodeEdgeGraphViewData[i];
                nodeSwitcherData.draw(this,graphView);
            }
        }
    }
    public class FlowchartGraphViewDataAsset : ScriptableObject
    {
        public FlowchartGraphViewData data { get; set; }

        public static FlowchartGraphViewDataAsset CreateInstance(FlowchartGraphViewData data = null)
        {
            var result = CreateInstance<FlowchartGraphViewDataAsset>();
            result.data = data == null ? new FlowchartGraphViewData() : data;
            return result;
        }
    }
}
