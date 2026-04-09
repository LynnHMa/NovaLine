using NovaLine.Script.Data.Edge;
using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Editor.Window.Context.Edge;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Editor.Graph.View
{
    using UnityEngine;
    using NovaLine.Script.Element;
    using NovaLine.Script.Editor.Graph.Node;
    using NovaLine.Script.Editor.Graph.Edge;
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
        public override IGraphViewNodeContext summonNewChildGraphViewNodeContext(NovaElement linkedElement, Vector2 pos)
        {
            return summonNewChildGraphViewNodeContext(new NodeData(linkedElement as Node, pos));
        }

        public override IGraphViewNodeContext summonNewChildGraphViewNodeContext(IGraphViewNodeData linkedData)
        {
            return new NodeNodeContext(linkedData as NodeData);
        }
        
        public override EdgeContext summonNewChildEdgeContext(NovaSwitcher linkedSwitcher)
        {
            return new EdgeContext(new NodeEdgeData(linkedSwitcher as NodeSwitcher));
        }
        
        public override IGraphEdge summonNewGraphEdge(NovaSwitcher linkedSwitcher)
        {
            return summonAndConnectEdge<NodeGraphEdge>((NodeSwitcher)linkedSwitcher);
        }
    }
}