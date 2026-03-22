namespace NovaLine.Editor.Graph.View
{
    using UnityEngine;
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Switcher;
    using NovaLine.Editor.File;
    using NovaLine.Editor.Graph.Edge;
    using NovaLine.Editor.Window.Context;
    using static NovaLine.Editor.Window.WindowContextRegistry;
    using NovaLine.Data.NodeGraphView;
    using NovaLine.Editor.Window.Command;
    using NovaLine.Editor.Window;

    public class FlowchartGraphView : NovaGraphView<NodeGraphNode,Flowchart,Node,NodeSwitcher>
    {
        public override NodeGraphNode firstNode
        {
            get
            {
                return base.firstNode;
            }
            set
            {
                base.firstNode = value;
                linkedElement.firstNode = (Node)value?.linkedElement;
            }
        }
        public FlowchartGraphView(Flowchart linkedFlowchart) : base(linkedFlowchart,linkedFlowchart?.name) {
        }
        protected override string getType()
        {
            return "[Flowchart]";
        }
        protected override void setNodePassable(NodeGraphNode graphNode)
        {
            if (graphNode == null || graphNode.isPassable || graphNode.linkedElement is not Node node || node.nextNodes == null) return;

            base.setNodePassable(graphNode);

            foreach (var nodeSwitcher in node.nextNodes)
            {
                if (nodeSwitcher.inputElement == null || nodeSwitcher.outputElement == null) continue;
                setNodePassable(getExistingGraphNode(nodeSwitcher.inputElement.guid));
            }
        }
        protected override void setNodeUnpassable(NodeGraphNode graphNode)
        {
            if (graphNode == null || !graphNode.isPassable) return;

            base.setNodeUnpassable(graphNode);

            if (graphNode == null || graphNode.linkedElement is not Node node || node.nextNodes == null) return;

            foreach (var nodeSwitcher in node.nextNodes)
            {
                if (nodeSwitcher.inputElement == null || nodeSwitcher.outputElement == null) continue;
                setNodeUnpassable(getExistingGraphNode(nodeSwitcher.inputElement.guid));
            }
        }
        public override NodeGraphNode summonNewGraphNode(Vector2 pos)
        {
            return new NodeGraphNode(new Node(linkedElement.nodes.Count.ToString()), pos);
        }
        public override NodeGraphNode summonNewGraphNode(Node node, Vector2 pos)
        {
            return new NodeGraphNode(node, pos);
        }
        public override IGraphViewContext summonNewChildGraphContext(Node node,Vector2 pos)
        {
            return new NodeContext(new NodeData(node, pos));
        }
        public override IGraphEdge summonNewGraphEdge(INovaSwitcher switcher)
        {
            return summonAndConnectEdge<NodeGraphEdge>((NodeSwitcher)switcher);
        }
        public override void addGraphEdge(IGraphEdge graphEdge, bool isLoading = false, bool autoSave = true, bool registerCommand = true)
        {
            base.addGraphEdge(graphEdge, isLoading, autoSave, registerCommand);

            if (!isLoading)
            {
                if (graphEdge is NodeGraphEdge nodeGraphEdge)
                {
                    RegisterContext(new ConditionContext(new ConditionData(nodeGraphEdge.linkedElement.switchCondition)));
                }
            }

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphEdge(IGraphEdge graphEdge, bool autoSave = true, bool registerCommand = true)
        {
            base.removeGraphEdge(graphEdge, autoSave, registerCommand);

            if(graphEdge.linkedElement != null) UnregisterContext(((NodeSwitcher)graphEdge.linkedElement).switchCondition.guid,NovaElementType.CONDITION);

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
                linkedElement.nodes.Add((Node)graphNode.linkedElement);
            }

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphNode(GraphNode graphNode, bool autoSave = true, bool registerCommand = true)
        {
            base.removeGraphNode(graphNode, autoSave, registerCommand);

            linkedElement.nodes.Remove((Node)graphNode.linkedElement);
            UnregisterContext(graphNode.guid, NovaElementType.NODE);

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
    }
}