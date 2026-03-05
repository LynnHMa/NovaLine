using System.Collections.Generic;
using UnityEngine;
using System;

namespace NovaLine.Editor.Graph.View
{
    using NovaLine.Action;
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Switcher;

    [Serializable]
    public class NodeGraphView : NovaGraphView<ActionGraphNode,NovaAction,ActionSwitcher>
    {
        public new Node root
        {
            get
            {
                return (Node)base.root;
            }
            set
            {
                base.root = value;
            }
        }
        public NodeGraphView(Node root) : base(root,root.name) { }
        protected override string getType()
        {
            return "[Node]";
        }
        public override ActionGraphNode summonNewGraphNode(Vector2 pos)
        {
            return new ActionGraphNode(new NovaAction(), pos);
        }
        public ActionGraphNode summonNewGraphNode(string title, Vector2 pos)
        {
            var newGraphNode = summonNewGraphNode(pos);
            newGraphNode.title = title;
            return newGraphNode;
        }
        public override void addGraphNode(ActionGraphNode graphNode, bool isInit = false)
        {
            base.addGraphNode(graphNode);
            if(!isInit) root.actions.Add((NovaAction)graphNode.targetObject);
        }
        public override void removeGraphNode(ActionGraphNode graphNode)
        {
            base.addGraphNode(graphNode);
            root.actions.Remove((NovaAction)graphNode.targetObject);
        }
    }
}