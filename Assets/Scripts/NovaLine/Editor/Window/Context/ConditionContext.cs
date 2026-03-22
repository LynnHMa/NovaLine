using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Data.Edge;
using NovaLine.Data.NodeGraphView;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.False, AsGraphView.True)]
    public class ConditionContext : GraphViewContext<ConditionGraphView, ConditionData>
    {
        public ConditionContext(ConditionData linkedData) : base(linkedData) { }
        public ConditionContext(ConditionGraphView graphView, ConditionData linkedData) : base(graphView, linkedData) { }

        public override void save()
        {
            base.save<EventGraphNode, EventContext, EventEdgeData>();
        }
        protected override void cleanInvalidChild()
        {
            if (linkedData != null && linkedData.linkedElement != null)
            {
                var novaEvent = linkedData.linkedElement.novaEvents;
                if(novaEvent == null || novaEvent.Count == 0) return;
                for (var i = novaEvent.Count - 1; i >= 0; i--)
                {
                    var action =  novaEvent[i];
                    if(action == null || linkedData.nodeDatas.Find(nD => nD.guid.Equals(action.guid)) == null) novaEvent.RemoveAt(i);
                }
            }
        }
        protected override ConditionGraphView summonGraphView()
        {
            return new ConditionGraphView(linkedData.linkedElement);
        }
    }
}
