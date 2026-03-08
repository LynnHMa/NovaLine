using System.Collections.Generic;
using UnityEngine;
using System;

namespace NovaLine.Editor.Graph.View
{
    using NovaLine.Action;
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Switcher;
    using System.Linq;
    using NovaLine.Editor.File;

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
        public override ActionGraphNode firstNode
        {
            get
            {
                return base.firstNode;
            }
            set
            {
                if(base.firstNode != null) base.firstNode.unmarkStartNode();
                base.firstNode = value;
                root.firstAction = (NovaAction)value.linkedElement;
                base.firstNode.markedAsStartNode();
            }
        }
        public NodeGraphView(Node root) : base(root,root.name) { }
        protected override string getType()
        {
            return "[Node]";
        }
        public override ActionGraphNode summonNewGraphNode(Vector2 pos)
        {
            var actualName = graphElements.Count().ToString();
            var newActionGraphNode = new ActionGraphNode(new NovaAction(actualName), pos);
            return newActionGraphNode;
        }
        public override void updateAllGraphElements()
        {
            for (var i = graphNodes.Count - 1; i >= 0; i--)
            {
                var actionGraphNode = graphNodes[i];
                if(actionGraphNode.linkedElement is NovaAction novaAction)
                {
                    if(novaAction.type == ActionType.Meanwhile && actionGraphNode.inputContainer.childCount > 0)
                    {
                        actionGraphNode.removePort();
                    }
                    else if(novaAction.type == ActionType.Sort && actionGraphNode.inputContainer.childCount == 0)
                    {
                        actionGraphNode.addPort();
                    }
                }
                actionGraphNode.title = actionGraphNode.linkedElement?.getActualName();
            }
        }
        public override void addGraphNode(ActionGraphNode graphNode, bool isInit = false, bool autoSave = true)
        {
            base.addGraphNode(graphNode, isInit, autoSave);

            if (!isInit) root.actions.Add((NovaAction)graphNode.linkedElement);

            if (autoSave)
            {
                NovaFileManager.saveGraphWindowData();
            }
        }
        public override void removeGraphNode(ActionGraphNode graphNode, bool autoSave = true)
        {
            base.removeGraphNode(graphNode, autoSave);

            root.actions.Remove((NovaAction)graphNode.linkedElement);

            if (autoSave)
            {
                NovaFileManager.saveGraphWindowData();
            }
        }
    }
}