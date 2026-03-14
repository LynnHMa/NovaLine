using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Editor.Graph.Data.NodeGraphView;
using NovaLine.Editor.Graph.Data.Edge;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.False, AsGraphView.True)]
    public class FlowchartContext : GraphViewContext<FlowchartGraphView, FlowchartData>
    {
        public FlowchartContext(FlowchartData linkedData) : base(linkedData) { }
        public FlowchartContext(FlowchartGraphView graphView, FlowchartData linkedData) : base(graphView, linkedData) { }

        public override void save()
        {
            base.save<NodeGraphNode, NodeContext, NodeEdgeData>();
        }

        protected override FlowchartGraphView summonGraphView()
        {
            return new FlowchartGraphView(linkedData.linkedElement);
        }
    }
}
