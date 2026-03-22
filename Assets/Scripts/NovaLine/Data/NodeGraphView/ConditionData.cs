using System;

namespace NovaLine.Data.NodeGraphView
{
    using NovaLine.Data.Edge;
    using NovaLine.Element;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class ConditionData : GraphViewNodeData<Condition, EventData, EventEdgeData>
    {
        [SerializeReference] private List<EventData> _nodeDatas = new();
        [SerializeReference] private List<EventEdgeData> _edgeDatas = new();
        [SerializeReference] private Condition _linkedElement;

        public override Condition linkedElement
        {
            get => _linkedElement;
            set => _linkedElement = value;
        }
        public override List<EventData> nodeDatas
        {
            get => _nodeDatas != null ? _nodeDatas : new();
            set => _nodeDatas = value != null ? value : _nodeDatas;
        }
        public override List<EventEdgeData> edgeDatas
        {
            get => _edgeDatas != null ? _edgeDatas : new();
            set => _edgeDatas = value != null ? value : _edgeDatas;
        }

        // Condition不存在节点，因此不需要存储pos
        public ConditionData(Condition linkedCondition)
        {
            linkedElement = linkedCondition;
        }
    }
}
