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
        public ConditionData ConditionBeforeInvokeData
        {
            get => _conditionBeforeInvokeData;
            set => _conditionBeforeInvokeData = value;
        }
        public ConditionData ConditionAfterInvokeData
        {
            get => _conditionAfterInvokeData;
            set => _conditionAfterInvokeData = value;
        }

        public NodeData(){}
        public NodeData(Node node,Vector2 pos)
        {
            this.pos = pos;
            linkedElement = node;
            ConditionBeforeInvokeData = new ConditionData(linkedElement?.ConditionBeforeInvoke);
            ConditionAfterInvokeData = new ConditionData(linkedElement?.ConditionAfterInvoke);
        }

        public override INovaData copy()
        {
            var clone = (NodeData)base.copy();
            
            if (clone == null) return null;
            
            clone.ConditionBeforeInvokeData = (ConditionData)ConditionBeforeInvokeData.copy();
            clone.ConditionAfterInvokeData = (ConditionData)ConditionAfterInvokeData.copy();
            clone.ConditionBeforeInvokeData.linkedElement = clone.linkedElement.ConditionBeforeInvoke;
            clone.ConditionAfterInvokeData.linkedElement = clone.linkedElement.ConditionAfterInvoke;
            return clone;
        }
        public override void updateLinkedElement(bool updateChildren = true)
        {
            if (updateChildren)
            {
                ConditionBeforeInvokeData?.updateLinkedElement();
                ConditionAfterInvokeData?.updateLinkedElement();
            }
            base.updateLinkedElement(updateChildren);
        }
        public override void registerLinkedElement()
        {
            ConditionBeforeInvokeData?.registerLinkedElement();
            ConditionAfterInvokeData?.registerLinkedElement();
            base.registerLinkedElement();
        }
    }
}
