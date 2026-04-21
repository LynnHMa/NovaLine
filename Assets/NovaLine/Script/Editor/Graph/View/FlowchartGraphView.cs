using NovaLine.Script.Data.Edge;
using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Window.Context.Edge;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element.Switcher;
using NovaLine.Script.Utils.Ext;

namespace NovaLine.Script.Editor.Graph.View
{
    using UnityEngine;
    using NovaLine.Script.Element;
    using NovaLine.Script.Editor.Graph.Node;
    using NovaLine.Script.Editor.Graph.Edge;
    using NovaLine.Script.Data.NodeGraphView;

    public class FlowchartGraphView : NovaGraphView<NodeGraphNode,Flowchart,Node,NodeSwitcher>
    {
        protected override Color ThemedColor => ColorExt.NODE_THEMED_COLOR;
        public FlowchartGraphView(string linkedFlowchartGuid) : base(linkedFlowchartGuid) { }
        public override NodeGraphNode SummonNewGraphNode(Vector2 pos)
        {
            var newNode = new Node((LinkedElement.ChildrenGuidList.Count + 1).ToString());
            return new NodeGraphNode(newNode, pos);
        }
        public override NodeGraphNode SummonNewGraphNode(Node node, Vector2 pos)
        {
            return new NodeGraphNode(node, pos);
        }
        public override IGraphViewNodeContext SummonNewChildGraphViewNodeContext(NovaElement linkedElement, Vector2 pos)
        {
            return SummonNewChildGraphViewNodeContext(new NodeData(linkedElement as Node, pos));
        }

        public override IGraphViewNodeContext SummonNewChildGraphViewNodeContext(IGraphViewNodeData linkedData)
        {
            return new NodeContext(linkedData as NodeData);
        }
        
        public override EdgeContext SummonNewChildEdgeContext(NovaSwitcher linkedSwitcher)
        {
            return new EdgeContext(new NodeEdgeData(linkedSwitcher as NodeSwitcher));
        }
        
        public override IGraphEdge SummonNewGraphEdge(NovaSwitcher linkedSwitcher)
        {
            return SummonAndConnectEdge<NodeGraphEdge>((NodeSwitcher)linkedSwitcher);
        }
    }
}