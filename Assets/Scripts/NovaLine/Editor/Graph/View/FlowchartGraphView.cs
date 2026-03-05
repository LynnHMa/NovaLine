using System.Collections.Generic;
using UnityEngine;
using System;

namespace NovaLine.Editor.Graph.View
{
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Switcher;

    [Serializable]
    public class FlowchartGraphView : NovaGraphView<NodeGraphNode,Node,NodeSwitcher>
    {
        public new Flowchart root
        {
            get
            {
                return (Flowchart)base.root;
            }
            set
            {
                base.root = value;
            }
        }
        public FlowchartGraphView(Flowchart root) : base(root,root.name) {
        }
        protected override string getType()
        {
            return "[Flowchart]";
        }
        public override NodeGraphNode summonNewGraphNode(Vector2 pos)
        {
            return new NodeGraphNode("" + graphNodes.Count, pos);
        }
        public NodeGraphNode summonNewGraphNode(string title, Vector2 pos)
        {
            var newGraphNode = summonNewGraphNode(pos);
            newGraphNode.title = title;
            return newGraphNode;
        }
        public override void addGraphNode(NodeGraphNode graphNode, bool isInit = false)
        {
            base.addGraphNode(graphNode);
            if(!isInit) root.nodes.Add((Node)graphNode.targetObject);
        }
        public override void removeGraphNode(NodeGraphNode graphNode)
        {
            base.addGraphNode(graphNode);
            root.nodes.Remove((Node)graphNode.targetObject);
        }
    }
}