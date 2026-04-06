using System;

namespace NovaLine.Script.Data.NodeGraphView
{
    using NovaLine.Script.Data.Edge;
    using NovaLine.Script.Element;

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
