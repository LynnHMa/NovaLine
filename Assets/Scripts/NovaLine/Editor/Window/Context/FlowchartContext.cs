using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Data.NodeGraphView;
using NovaLine.Data.Edge;
using NovaLine.Element;

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
        protected override void cleanInvalidChild()
        {
            if (linkedData != null && linkedData.linkedElement != null)
            {
                var nodes = linkedData.linkedElement.nodes;
                if(nodes == null || nodes.Count == 0) return;
                for (var i = nodes.Count - 1; i >= 0; i--)
                {
                    var action =  nodes[i];
                    if(action == null || linkedData.nodeDatas.Find(nD => nD.guid.Equals(action.guid)) == null) nodes.RemoveAt(i);
                }
            }
        }
        protected override FlowchartGraphView summonGraphView()
        {
            return new FlowchartGraphView(linkedData.linkedElement);
        }
    }
}
