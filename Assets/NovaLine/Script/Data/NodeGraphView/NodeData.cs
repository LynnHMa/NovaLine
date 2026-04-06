using System;
using System.Collections.Generic;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Data.NodeGraphView
{
    using NovaLine.Script.Element;
    using UnityEngine;
    using NovaLine.Script.Data.Edge;

    [Serializable]
    public class NodeData : GraphViewNodeData<Node, ActionData, ActionEdgeData>,IHasConditionData
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
        public override void registerLinkedElement()
        {
            conditionBeforeInvokeData.registerLinkedElement();
            conditionAfterInvokeData.registerLinkedElement();
            base.registerLinkedElement();
        }
    }
}
