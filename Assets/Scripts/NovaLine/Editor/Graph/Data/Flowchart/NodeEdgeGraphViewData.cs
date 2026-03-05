using NovaLine.Element;
using NovaLine.Editor.Graph.Node;
using NovaLine.Switcher;
using System;

namespace NovaLine.Editor.Graph.Data
{
    [Serializable]
    public class NodeEdgeGraphViewData : GraphViewEdgeData<Element.Node,NodeSwitcher, FlowchartGraphViewData, Flowchart,NodeGraphNode>
    {
        public Condition switchCondition { get; set; }

        public string inputNodeGUID { get; set; }

        public string outputNodeGUID { get; set; }

        public override string name => "Next Node";

        public override string describtion => "Set next node and its condition.";
        public NodeEdgeGraphViewData(NodeSwitcher switcher) : base(switcher)
        {
            switchCondition = switcher.switchCondition;
            inputNodeGUID = switcher.inputElement.guid;
            outputNodeGUID = switcher.outputElement.guid;
        }
        public override NodeSwitcher to(FlowchartGraphViewData flowchartGraphViewData)
        {
            Element.Node inputNode = null;
            Element.Node outputNode = null;
            foreach(var nodeData in flowchartGraphViewData?.nodeGraphViewDatas)
            {
                if (nodeData.guid.Equals(inputNodeGUID)) inputNode = nodeData.to();
                else if (nodeData.guid.Equals(outputNodeGUID)) outputNode = nodeData.to();
                if (inputNode != null && outputNode != null) break;
            }
            var result = new NodeSwitcher(switchCondition, inputNode, outputNode, guid);
            return result;
        }
    }
}
