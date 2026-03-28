using System.Collections.Generic;
using System.Linq;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Data.NodeGraphView;
using NovaLine.Data.Edge;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Element;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.False, AsGraphView.True)]
    public class FlowchartContext : GraphViewContext<FlowchartGraphView, FlowchartData>
    {
        public FlowchartContext(FlowchartData linkedData) : base(linkedData) { }
        public override void saveNodeData(List<GraphNode> graphNodes = null)
        {
            saveNodeData<NodeGraphNode, NodeContext>(graphNodes == null ? null : graphNodes.Cast<NodeGraphNode>().ToList());
        }
        public override void saveEdgeData(List<IGraphEdge> graphEdges = null)
        {
            saveEdgeData<NodeEdgeData>(graphEdges);
        }
        protected override FlowchartGraphView summonGraphView()
        {
            return new FlowchartGraphView(linkedData.linkedElement);
        }
    }
}
