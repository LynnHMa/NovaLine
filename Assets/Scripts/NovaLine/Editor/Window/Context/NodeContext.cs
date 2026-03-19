using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Editor.Graph.Data.NodeGraphView;
using NovaLine.Editor.Graph.Data.Edge;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.True, AsGraphView.True)]
    public class NodeContext : GraphViewContext<NodeGraphView, NodeData>
    {
        public override ContextType type => ContextType.NODE;
        public NodeContext(NodeData linkedData) : base(linkedData) { }
        public NodeContext(NodeGraphView graphView, NodeData linkedData) : base(graphView, linkedData) { }

        public override void save()
        {
            base.save<ActionGraphNode, ActionContext, ActionEdgeData>();
        }
        protected override NodeGraphView summonGraphView()
        {
            return new NodeGraphView(linkedData.linkedElement);
        }
    }
}
