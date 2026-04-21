using System.Collections.Generic;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Utils.Interface;
using static NovaLine.Script.Editor.Window.Context.GraphViewNode.ContextInfo;

namespace NovaLine.Script.Editor.Window.Context.GraphViewNode
{
    [ContextInfo(AsNode.True, AsGraphView.False)]
    public class EventContext : GraphViewNodeContext<IObjectEditor, EventData>
    {
        public EventContext(EventData linkedData) : base(linkedData) { }
        public override void SaveData()
        {
        }
        public override void SaveNodeData(List<GraphNode> graphNodes = null)
        {
        }

        public override void SaveEdgeData(List<IGraphEdge> graphEdges = null)
        {
        }
        //No event graph view
        protected override IObjectEditor SummonGraphView()
        {
            return null;
        }
    }
}
