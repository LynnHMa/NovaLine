using NovaLine.Element;
using NovaLine.Editor.Graph.View;
using NovaLine.Interface;
using System;
using System.Collections.Generic;
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
        public FlowchartGraphViewData(FlowchartGraphView flowchartGraphView) : this(flowchartGraphView.root)
        {
            foreach(var graphNode in flowchartGraphView.graphNodes)
            {
                var node = (Element.Node)graphNode.targetObject;
                nodeGraphViewDatas.Add(new NodeGraphViewData(node,graphNode.pos));
                foreach (var nodeSwitcher in node.nextNodes)
                {
                    nodeEdgeGraphViewData.Add(new NodeEdgeGraphViewData(nodeSwitcher));
                }
            }
        }
        public FlowchartGraphViewData(Flowchart flowchart)
        {
            guid = flowchart.guid;
            name = flowchart.name;
            describtion = flowchart.describtion;
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
                    if (nodeSwitcher.outputElement.guid == node.guid)
                    {
                        node.nextNodes.Add(nodeSwitcher);
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
}
