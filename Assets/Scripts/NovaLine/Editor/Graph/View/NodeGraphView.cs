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
            var newActionGraphNode = new ActionGraphNode(new NovaAction(), pos);
            newActionGraphNode.targetObject.name = UnityEngine.Random.Range(0, 88888).ToString();
            return newActionGraphNode;
        }
        public ActionGraphNode summonNewGraphNode(string title, Vector2 pos)
        {
            var newActionGraphNode = summonNewGraphNode(pos);
            newActionGraphNode.targetObject.name = title;
            return newActionGraphNode;
        }
        public override void addGraphNode(ActionGraphNode graphNode, bool isInit = false,bool autoSave = true)
        {
            base.addGraphNode(graphNode);
            if(!isInit) root.actions.Add((NovaAction)graphNode.targetObject);
        }
        public override void removeGraphNode(ActionGraphNode graphNode, bool isSave = true)
        {
            base.addGraphNode(graphNode);
            root.actions.Remove((NovaAction)graphNode.targetObject);
        }
    }
}