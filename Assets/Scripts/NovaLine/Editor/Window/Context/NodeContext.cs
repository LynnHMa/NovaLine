using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using NovaLine.Data.Edge;
using NovaLine.Data.NodeGraphView;
using static NovaLine.Editor.Window.Context.ContextInfo;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.True, AsGraphView.True)]
    public class NodeContext : GraphViewContext<NodeGraphView, NodeData>
    {
        public NodeContext(NodeData linkedData) : base(linkedData) { }
        public NodeContext(NodeGraphView graphView, NodeData linkedData) : base(graphView, linkedData) { }

        public override void save()
        {
            base.save<ActionGraphNode, ActionContext, ActionEdgeData>();
        }

        protected override void cleanInvalidChild()
        {
            if (linkedData != null && linkedData.linkedElement != null)
            {
                var actions = linkedData.linkedElement.actions;
                if(actions == null || actions.Count == 0) return;
                for (var i = actions.Count - 1; i >= 0; i--)
                {
                    var action =  actions[i];
                    if(action == null || linkedData.nodeDatas.Find(nD => nD.guid.Equals(action.guid)) == null) actions.RemoveAt(i);
                }
            }
        }

        protected override NodeGraphView summonGraphView()
        {
            return new NodeGraphView(linkedData.linkedElement);
        }
    }
}
