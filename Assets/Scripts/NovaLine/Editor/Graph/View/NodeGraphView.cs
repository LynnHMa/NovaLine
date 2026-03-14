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
    using NovaLine.Editor.Graph.Edge;
    using NovaLine.Editor.Window.Context;
    using NovaLine.Editor.Graph.Data.NodeGraphView;

    public class NodeGraphView : NovaGraphView<ActionGraphNode,Node,NovaAction,ActionSwitcher>
    {
        public override ActionGraphNode firstNode
        {
            get
            {
                return base.firstNode;
            }
            set
            {
                base.firstNode = value;
                root.firstAction = (NovaAction)value.linkedElement;
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
        public override ActionGraphNode summonNewGraphNode(NovaAction novaAction, Vector2 pos)
        {
            return new ActionGraphNode(novaAction, pos);
        }
        public override IGraphViewContext summonNewChildGraphContext(NovaAction action, Vector2 pos)
        {
            return new ActionContext(new ActionData(action, pos));
        }
        public override IGraphEdge summonNewGraphEdge(INovaSwitcher switcher)
        {
            return summonAndConnectEdge<ActionGraphEdge>((ActionSwitcher)switcher);
        }
        public override void addGraphEdge(IGraphEdge graphEdge, bool isLoading = false, bool autoSave = true)
        {
            base.addGraphEdge(graphEdge, isLoading, autoSave);

            if (autoSave)
            {
                NovaFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphEdge(IGraphEdge graphEdge, bool autoSave = true)
        {
            base.removeGraphEdge(graphEdge, autoSave);

            if (autoSave)
            {
                NovaFileManager.SaveGraphWindowData();
            }
        }
        public override void addGraphNode(GraphNode graphNode, bool isLoading = false, bool autoSave = true)
        {
            base.addGraphNode(graphNode, isLoading, autoSave);

            if (!isLoading)
            {
                root.actions.Add((NovaAction)graphNode.linkedElement);
                
            }

            if (autoSave)
            {
                NovaFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphNode(GraphNode graphNode, bool autoSave = true)
        {
            base.removeGraphNode(graphNode, autoSave);

            root.actions.Remove((NovaAction)graphNode.linkedElement);

            if (autoSave)
            {
                NovaFileManager.SaveGraphWindowData();
            }
        }
    }
}