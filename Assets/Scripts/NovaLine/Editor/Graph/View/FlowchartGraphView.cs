using NovaLine.Editor.Utils;
using NovaLine.Element.Switcher;

namespace NovaLine.Editor.Graph.View
{
    using UnityEngine;
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Editor.Graph.Edge;
    using NovaLine.Editor.Window.Context;
    using static NovaLine.Editor.Window.WindowContextRegistry;
    using NovaLine.Data.NodeGraphView;

    public class FlowchartGraphView : NovaGraphView<NodeGraphNode,Flowchart,Node,NodeSwitcher>
    {
        protected override Color themedColor => ColorExt.NODE_THEMED_COLOR;
        public FlowchartGraphView(Flowchart linkedFlowchart) : base(linkedFlowchart) { }
        public override NodeGraphNode summonNewGraphNode(Vector2 pos)
        {
            return new NodeGraphNode(new Node(linkedElement.children.Count.ToString()), pos);
        }
        public override NodeGraphNode summonNewGraphNode(Node node, Vector2 pos)
        {
            return new NodeGraphNode(node, pos);
        }
        public override IGraphViewContext summonNewChildGraphContext(NovaElement node,Vector2 pos)
        {
            return new NodeContext(new NodeData((Node)node, pos));
        }
        public override IGraphEdge summonNewGraphEdge(INovaSwitcher switcher)
        {
            return summonAndConnectEdge<NodeGraphEdge>((NodeSwitcher)switcher);
        }
        public override void addGraphEdge(IGraphEdge graphEdge, bool registerCommand = true)
        {
            base.addGraphEdge(graphEdge, registerCommand);

            if (graphEdge is NodeGraphEdge nodeGraphEdge)
            {
                RegisterContext(new ConditionContext(new ConditionData(nodeGraphEdge.linkedElement.switchCondition)));
            }
        }
    }
}