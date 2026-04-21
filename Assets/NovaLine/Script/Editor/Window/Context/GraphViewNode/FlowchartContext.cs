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
    [ContextInfo(AsNode.False, AsGraphView.True)]
    public class FlowchartContext : GraphViewNodeContext<FlowchartGraphView, FlowchartData>
    {
        public FlowchartContext(FlowchartData linkedData) : base(linkedData) { }
        public override void SaveData()
        {
            base.SaveData();
            LinkedData.UpdateLinkedElement();
        }
        public override void SaveNodeData(List<GraphNode> graphNodes = null)
        {
            SaveNodeData<NodeGraphNode, NodeContext>(graphNodes == null ? null : graphNodes.Cast<NodeGraphNode>().ToList());
        }
        public override void SaveEdgeData(List<IGraphEdge> graphEdges = null)
        {
            SaveEdgeData<NodeEdgeData>(graphEdges);
        }
        protected override FlowchartGraphView SummonGraphView()
        {
            return new FlowchartGraphView(LinkedData.Guid);
        }
    }
}
