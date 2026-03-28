using System.Collections.Generic;
using System.Linq;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using NovaLine.Data.Edge;
using NovaLine.Data.NodeGraphView;
using NovaLine.Editor.Graph.Edge;
using static NovaLine.Editor.Window.Context.ContextInfo;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.True, AsGraphView.True)]
    public class NodeContext : GraphViewContext<NodeGraphView, NodeData>
    {
        public NodeContext(NodeData linkedData) : base(linkedData) { }
        public override void saveNodeData(List<GraphNode> graphNodes = null)
        {
            saveNodeData<ActionGraphNode, ActionContext>(graphNodes == null ? null : graphNodes.Cast<ActionGraphNode>().ToList());
        }
        public override void saveEdgeData(List<IGraphEdge> graphEdges = null)
        {
            saveEdgeData<ActionEdgeData>(graphEdges);
        }
        protected override NodeGraphView summonGraphView()
        {
            return new NodeGraphView(linkedData.linkedElement);
        }
    }
}
