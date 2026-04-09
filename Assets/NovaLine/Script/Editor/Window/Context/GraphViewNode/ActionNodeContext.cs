using System.Collections.Generic;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Utils.Interface;
using static NovaLine.Script.Editor.Window.Context.GraphViewNode.ContextInfo;

namespace NovaLine.Script.Editor.Window.Context.GraphViewNode
{
    [ContextInfo(AsNode.True, AsGraphView.False)]
    public class ActionNodeContext : GraphViewNodeContext<IObjectEditor, ActionData>
    {
        public ActionNodeContext(ActionData linkedData) : base(linkedData) { }
        public override void saveData()
        {
        }

        public override void saveNodeData(List<GraphNode> graphNodes = null)
        {
        }

        public override void saveEdgeData(List<IGraphEdge> graphEdges = null)
        {
        }

        //No action graph view
        protected override IObjectEditor summonGraphView()
        {
            return null;
        }
    }
}
