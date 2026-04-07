using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Editor.Graph.View
{
    using UnityEngine;
    using NovaLine.Script.Element;
    using NovaLine.Script.Editor.Graph.Node;
    using NovaLine.Script.Editor.Graph.Edge;
    using NovaLine.Script.Editor.Window.Context;
    using static NovaLine.Script.Editor.Window.ContextRegistry;
    using NovaLine.Script.Data.NodeGraphView;

    public class FlowchartGraphView : NovaGraphView<NodeGraphNode,Flowchart,Node,NodeSwitcher>
    {
        protected override Color themedColor => ColorExt.NODE_THEMED_COLOR;
        public FlowchartGraphView(string linkedFlowchartGuid) : base(linkedFlowchartGuid) { }
        public override NodeGraphNode summonNewGraphNode(Vector2 pos)
        {
            var newNode = new Node((linkedElement.childrenGuidList.Count + 1).ToString());
            return new NodeGraphNode(newNode, pos);
        }
        public override NodeGraphNode summonNewGraphNode(Node node, Vector2 pos)
        {
            return new NodeGraphNode(node, pos);
        }
        public override IGraphViewContext summonNewChildGraphContext(NovaElement node,Vector2 pos)
        {
            return new NodeContext(new NodeData((Node)node, pos));
        }
        public override IGraphEdge summonNewGraphEdge(NovaSwitcher linkedSwitcher)
        {
            return summonAndConnectEdge<NodeGraphEdge>((NodeSwitcher)linkedSwitcher);
        }
        public override void addGraphEdge(IGraphEdge graphEdge, bool registerCommand = true)
        {
            base.addGraphEdge(graphEdge, registerCommand);

            if (graphEdge is NodeGraphEdge nodeGraphEdge)
            {
                RegisterContext(new ConditionContext(new ConditionData(nodeGraphEdge.linkedElement.switchCondition)));
            }
        }

        public override void removeGraphEdge(IGraphEdge graphEdge, bool registerCommand = true)
        {
            if (graphEdge is NodeGraphEdge nodeGraphEdge)
            {
                UnregisterContext(nodeGraphEdge.linkedElement.switchConditionGuid,NovaElementType.CONDITION);
            }
            base.removeGraphEdge(graphEdge, registerCommand);
        }
    }
}