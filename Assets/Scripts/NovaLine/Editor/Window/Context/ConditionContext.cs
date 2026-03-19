using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Editor.Graph.Data.NodeGraphView;
using NovaLine.Editor.Graph.Data.Edge;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.False, AsGraphView.True)]
    public class ConditionContext : GraphViewContext<ConditionGraphView, ConditionData>
    {
        public override ContextType type => ContextType.CONDITION;
        public ConditionContext(ConditionData linkedData) : base(linkedData) { }
        public ConditionContext(ConditionGraphView graphView, ConditionData linkedData) : base(graphView, linkedData) { }

        public override void save()
        {
            base.save<EventGraphNode, EventContext, EventEdgeData>();
        }

        protected override ConditionGraphView summonGraphView()
        {
            return new ConditionGraphView(linkedData.linkedElement);
        }
    }
}
