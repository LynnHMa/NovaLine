using NovaLine.Element;
using NovaLine.Editor.Graph.Node;
using NovaLine.Switcher;
using System;

namespace NovaLine.Editor.Graph.Data
{
    [Serializable]
    public class NodeEdgeGraphViewData : GraphViewEdgeData<Element.Node,NodeSwitcher, FlowchartGraphViewData, Flowchart,NodeGraphNode>
    {
        public override string name => "Next Node";

        public override string describtion => "Set next node and its condition.";
        public NodeEdgeGraphViewData(NodeSwitcher novaSwitcher) : base(novaSwitcher)
        {
        }
    }
}
