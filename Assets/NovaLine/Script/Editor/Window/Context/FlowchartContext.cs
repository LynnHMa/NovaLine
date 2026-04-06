using System.Collections.Generic;
using System.Linq;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Graph.View;
using static NovaLine.Script.Editor.Window.Context.ContextInfo;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Editor.Graph.Edge;

namespace NovaLine.Script.Editor.Window.Context
{
    [ContextInfo(AsNode.False, AsGraphView.True)]
    public class FlowchartContext : GraphViewContext<FlowchartGraphView, FlowchartData>
    {
        public FlowchartContext(FlowchartData linkedData) : base(linkedData) { }
        public override void saveData()
        {
            base.saveData();
            linkedData.updateLinkedElement();
        }
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
            return new FlowchartGraphView(linkedData.guid);
        }
    }
}
