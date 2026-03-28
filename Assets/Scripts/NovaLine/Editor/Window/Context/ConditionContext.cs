using System.Collections.Generic;
using System.Linq;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Data.Edge;
using NovaLine.Data.NodeGraphView;
using NovaLine.Editor.Graph.Edge;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.False, AsGraphView.True)]
    public class ConditionContext : GraphViewContext<ConditionGraphView, ConditionData>
    {
        public ConditionContext(ConditionData linkedData) : base(linkedData) { }

        public override void saveNodeData(List<GraphNode> graphNodes = null)
        {
            saveNodeData<EventGraphNode, EventContext>(graphNodes == null ? null : graphNodes.Cast<EventGraphNode>().ToList());
        }

        public override void saveEdgeData(List<IGraphEdge> graphEdges = null)
        {
            saveEdgeData<EventEdgeData>(graphEdges);
        }

        protected override ConditionGraphView summonGraphView()
        {
            return new ConditionGraphView(linkedData.linkedElement);
        }
    }
}
