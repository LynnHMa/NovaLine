using System;
using System.Collections.Generic;

namespace NovaLine.Data.NodeGraphView
{
    using NovaLine.Element;
    using UnityEngine;
    using NovaLine.Data.Edge;

    [Serializable]
    public class NodeData : GraphViewNodeData<Node, ActionData, ActionEdgeData>
    {
        [SerializeReference] private ConditionData _conditionBeforeInvokeData;
        [SerializeReference] private ConditionData _conditionAfterInvokeData;
        public ConditionData conditionBeforeInvokeData
        {
            get => _conditionBeforeInvokeData;
            set => _conditionBeforeInvokeData = value;
        }
        public ConditionData conditionAfterInvokeData
        {
            get => _conditionAfterInvokeData;
            set => _conditionAfterInvokeData = value;
        }

        public NodeData(){}
        public NodeData(Node node,Vector2 pos)
        {
            this.pos = pos;
            linkedElement = node;
            conditionBeforeInvokeData = new ConditionData(linkedElement?.conditionBeforeInvoke);
            conditionAfterInvokeData = new ConditionData(linkedElement?.conditionAfterInvoke);
        }

        public override INovaData copy()
        {
            var clone = (NodeData)base.copy();
            
            if (clone == null) return null;
            
            clone.conditionBeforeInvokeData = (ConditionData)conditionBeforeInvokeData.copy();
            clone.conditionAfterInvokeData = (ConditionData)conditionAfterInvokeData.copy();
            clone.conditionBeforeInvokeData.linkedElement = clone.linkedElement.conditionBeforeInvoke;
            clone.conditionAfterInvokeData.linkedElement = clone.linkedElement.conditionAfterInvoke;
            return clone;
        }
    }
}
