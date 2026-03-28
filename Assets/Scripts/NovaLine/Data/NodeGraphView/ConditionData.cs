using System;

namespace NovaLine.Data.NodeGraphView
{
    using NovaLine.Data.Edge;
    using NovaLine.Element;

    [Serializable]
    public class ConditionData : GraphViewNodeData<Condition, EventData, EventEdgeData>
    {
        public ConditionData(){}
        public ConditionData(Condition linkedCondition)
        {
            linkedElement = linkedCondition;
        }
    }
}
