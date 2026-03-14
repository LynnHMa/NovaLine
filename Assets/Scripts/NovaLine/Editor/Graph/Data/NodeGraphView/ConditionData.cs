using System;

namespace NovaLine.Editor.Graph.Data.NodeGraphView
{
    using NovaLine.Editor.Graph.Data.Edge;
    using NovaLine.Element;
    using NovaLine.Utils;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class ConditionData : GraphViewNodeData<Condition,EventData, EventEdgeData>
    {
        [SerializeReference] private List<EventData> _nodeDatas = new();
        [SerializeReference] private List<EventEdgeData> _edgeDatas = new();

        public override EList<EventData> nodeDatas
        {
            get => _nodeDatas != null ? new (_nodeDatas) : new();
            set => _nodeDatas = value != null ? new(value) : new();
        }
        public override EList<EventEdgeData> edgeDatas
        {
            get => _edgeDatas != null ? new(_edgeDatas) : new();
            set => _edgeDatas = value != null ? new(value) : new();
        }

        // Condition不存在节点，因此不需要存储pos
        public ConditionData(Condition linkedCondition)
        {
            linkedElement = linkedCondition;
        }
    }
}
