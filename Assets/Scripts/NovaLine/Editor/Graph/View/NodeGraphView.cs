using NovaLine.Element.Switcher;
using UnityEngine;
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
    using static NovaLine.Editor.Window.WindowContextRegistry;
    using NovaLine.Data.NodeGraphView;

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
                linkedElement.firstAction = (NovaAction)value?.linkedElement;
            }
        }
        public NodeGraphView(Node linkedNode) : base(linkedNode,linkedNode?.name) { }
        protected override string getType()
        {
            return "[Node]";
        }
        protected override void setNodePassable(ActionGraphNode graphNode)
        {
            if (graphNode == null || graphNode.isPassable || graphNode.linkedElement is not NovaAction action) return;

            base.setNodePassable(graphNode);

            if (action.nextAction == null || action.actionType != ActionType.Sort) return;

            var actionSwitcher = action.nextAction;

            if (actionSwitcher.inputElement == null || actionSwitcher.outputElement == null) return;

            setNodePassable(getExistingGraphNode(actionSwitcher.inputElement.guid));
        }
        protected override void setNodeUnpassable(ActionGraphNode graphNode)
        {
            if (graphNode == null || !graphNode.isPassable || graphNode.linkedElement is not NovaAction action || action.actionType != ActionType.Sort) return;

            base.setNodeUnpassable(graphNode);

            if (action.nextAction == null) return;

            var actionSwitcher = action.nextAction;

            if (actionSwitcher.inputElement == null || actionSwitcher.outputElement == null) return;

            setNodeUnpassable(getExistingGraphNode(actionSwitcher.inputElement.guid));
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
        protected override void updateNodes()
        {
            base.updateNodes();

            foreach(var graphNode in graphNodes)
            {
                if(graphNode.linkedElement is NovaAction action && action.actionType == ActionType.Meanwhile)
                {
                    setNodePassable(graphNode);
                }
            }
        }
        public override void addGraphEdge(IGraphEdge graphEdge, bool isLoading = false, bool autoSave = true, bool registerCommand = true)
        {
            base.addGraphEdge(graphEdge, isLoading, autoSave, registerCommand);

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphEdge(IGraphEdge graphEdge, bool autoSave = true, bool registerCommand = true)
        {
            base.removeGraphEdge(graphEdge, autoSave, registerCommand);

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
        public override void addGraphNode(GraphNode graphNode, bool isLoading = false, bool autoSave = true, bool registerCommand = true)
        {
            base.addGraphNode(graphNode, isLoading, autoSave, registerCommand);

            if (!isLoading)
            {
                linkedElement.actions.Add((NovaAction)graphNode.linkedElement);
            }

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphNode(GraphNode graphNode, bool autoSave = true, bool registerCommand = true)
        {
            base.removeGraphNode(graphNode, autoSave, registerCommand);

            linkedElement.actions.Remove((NovaAction)graphNode.linkedElement);
            UnregisterContext(graphNode.guid, NovaElementType.ACTION);

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
    }
}