namespace NovaLine.Editor.Graph.View
{
    using UnityEngine;
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Switcher;
    using NovaLine.Editor.File;
    using NovaLine.Editor.Graph.Edge;
    using NovaLine.Editor.Window.Context;
    using NovaLine.Editor.Graph.Data.NodeGraphView;

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
                linkedElement.firstNode = (Node)value.linkedElement;
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
            var newGraphView = new NodeGraphView(node);
            return new NodeContext(newGraphView,new NodeData(newGraphView,pos));
        }
        public override IGraphEdge summonNewGraphEdge(INovaSwitcher switcher)
        {
            return summonAndConnectEdge<NodeGraphEdge>((NodeSwitcher)switcher);
        }
        public override void addGraphEdge(IGraphEdge graphEdge, bool isLoading = false, bool autoSave = true)
        {
            base.addGraphEdge(graphEdge, isLoading, autoSave);

            if (!isLoading)
            {
                if (graphEdge is NodeGraphEdge nodeGraphEdge)
                {
                    NovaWindow.RegisterContext(new ConditionContext(new ConditionData(nodeGraphEdge.linkedElement.switchCondition)));
                }
            }

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphEdge(IGraphEdge graphEdge, bool autoSave = true)
        {
            base.removeGraphEdge(graphEdge, autoSave);

            NovaWindow.UnregisterContext(((NodeSwitcher)graphEdge.linkedElement).switchCondition.guid,ContextType.CONDITION);

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
        public override void addGraphNode(GraphNode graphNode, bool isLoading = false, bool autoSave = true)
        {
            base.addGraphNode(graphNode, isLoading, autoSave);

            if (!isLoading)
            {
                linkedElement.nodes.Add((Node)graphNode.linkedElement);
            }

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphNode(GraphNode graphNode, bool autoSave = true)
        {
            base.removeGraphNode(graphNode, autoSave);

            linkedElement.nodes.Remove((Node)graphNode.linkedElement);
            NovaWindow.UnregisterContext(graphNode.guid, ContextType.NODE);

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
    }
}