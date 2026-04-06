using NovaLine.Script.Data.Edge;
using NovaLine.Script.Element;
using System;

namespace NovaLine.Script.Data.NodeGraphView
{
    [Serializable]
    public class FlowchartData : GraphViewNodeData<Flowchart, NodeData, NodeEdgeData>
    {
        public FlowchartData()
        {
            linkedElement = new Flowchart("New Flowchart");
        }

        public override void registerLinkedElement()
        {
            NovaElementRegistry.Clear();
            base.registerLinkedElement();
        }
    }
}
