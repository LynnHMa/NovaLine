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
    public class NodeNodeContext : GraphViewNodeContext<NodeGraphView, NodeData>
    {
        public NodeNodeContext(NodeData linkedData) : base(linkedData) { }
        public override void saveNodeData(List<GraphNode> graphNodes = null)
        {
            saveNodeData<ActionGraphNode, ActionNodeContext>(graphNodes == null ? null : graphNodes.Cast<ActionGraphNode>().ToList());
        }
        public override void saveEdgeData(List<IGraphEdge> graphEdges = null)
        {
            saveEdgeData<ActionEdgeData>(graphEdges);
        }
        protected override NodeGraphView summonGraphView()
        {
            return new NodeGraphView(linkedData.guid);
        }
    }
}
