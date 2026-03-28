using NovaLine.Data.Edge;
using NovaLine.Element;
using System;

namespace NovaLine.Data.NodeGraphView
{
    [Serializable]
    public class FlowchartData : GraphViewNodeData<Flowchart, NodeData, NodeEdgeData>
    {
        public FlowchartData()
        {
            linkedElement = new Flowchart("New Flowchart");
        }
    }
}
