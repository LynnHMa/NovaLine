using System.Collections.Generic;
using UnityEngine;
using System;

namespace NovaLine.Editor.Graph.View
{
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Switcher;
    using NovaLine.Editor.File;

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
        public override NodeGraphNode firstNode
        {
            get
            {
                return base.firstNode;
            }
            set
            {
                base.firstNode?.unmarkStartNode();
                base.firstNode = value;
                root.firstNode = (Node)value.linkedElement;
                base.firstNode.markedAsStartNode();
                NovaFileManager.saveGraphWindowData();
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
            return new NodeGraphNode(new Node(root.nodes.Count.ToString()), pos);
        }
        public NodeGraphNode summonNewGraphNode(string title, Vector2 pos)
        {
            var newGraphNode = summonNewGraphNode(pos);
            newGraphNode.name = title;
            newGraphNode.title = getType() + title;
            return newGraphNode;
        }
        public override void addGraphNode(NodeGraphNode graphNode, bool isInit = false, bool autoSave = true)
        {
            base.addGraphNode(graphNode, isInit, autoSave);

            if (!isInit) root.nodes.Add((Node)graphNode.linkedElement);

            if (autoSave)
            {
                NovaFileManager.saveGraphWindowData();
            }
        }
        public override void removeGraphNode(NodeGraphNode graphNode, bool autoSave = true)
        {
            base.removeGraphNode(graphNode, autoSave);

            root.nodes.Remove((Node)graphNode.linkedElement);

            if (autoSave)
            {
                NovaFileManager.saveGraphWindowData();
            }
        }
    }
}