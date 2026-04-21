using System.Collections.Generic;
using System.Linq;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Graph.View;
using static NovaLine.Script.Editor.Window.Context.GraphViewNode.ContextInfo;

namespace NovaLine.Script.Editor.Window.Context.GraphViewNode
{
    [ContextInfo(AsNode.True, AsGraphView.True)]
    public class NodeContext : GraphViewNodeContext<NodeGraphView, NodeData>
    {
        public NodeContext(NodeData linkedData) : base(linkedData) { }
        
        public override void SaveNodeData(List<GraphNode> graphNodes = null)
        {
            SaveNodeData<ActionGraphNode, ActionContext>(graphNodes == null ? null : graphNodes.Cast<ActionGraphNode>().ToList());
        }
        public override void SaveEdgeData(List<IGraphEdge> graphEdges = null)
        {
            SaveEdgeData<ActionEdgeData>(graphEdges);
        }
        protected override NodeGraphView SummonGraphView()
        {
            return new NodeGraphView(LinkedData.Guid);
        }
    }
}
