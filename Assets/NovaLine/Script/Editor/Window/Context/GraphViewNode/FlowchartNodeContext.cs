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
    public class FlowchartNodeContext : GraphViewNodeContext<FlowchartGraphView, FlowchartData>
    {
        public FlowchartNodeContext(FlowchartData linkedData) : base(linkedData) { }
        public override void saveData()
        {
            base.saveData();
            linkedData.updateLinkedElement();
        }
        public override void saveNodeData(List<GraphNode> graphNodes = null)
        {
            saveNodeData<NodeGraphNode, NodeNodeContext>(graphNodes == null ? null : graphNodes.Cast<NodeGraphNode>().ToList());
        }
        public override void saveEdgeData(List<IGraphEdge> graphEdges = null)
        {
            saveEdgeData<NodeEdgeData>(graphEdges);
        }
        protected override FlowchartGraphView summonGraphView()
        {
            return new FlowchartGraphView(linkedData.guid);
        }
    }
}
