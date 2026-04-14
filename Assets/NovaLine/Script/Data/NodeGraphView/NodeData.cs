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
            this.Pos = pos;
            LinkedElement = node;
            ConditionBeforeInvokeData = new ConditionData(LinkedElement?.ConditionBeforeInvoke);
            ConditionAfterInvokeData = new ConditionData(LinkedElement?.ConditionAfterInvoke);
        }

        public override INovaData Copy()
        {
            var clone = (NodeData)base.Copy();
            
            if (clone == null) return null;
            
            clone.ConditionBeforeInvokeData = (ConditionData)ConditionBeforeInvokeData.Copy();
            clone.ConditionAfterInvokeData = (ConditionData)ConditionAfterInvokeData.Copy();
            clone.ConditionBeforeInvokeData.LinkedElement = clone.LinkedElement.ConditionBeforeInvoke;
            clone.ConditionAfterInvokeData.LinkedElement = clone.LinkedElement.ConditionAfterInvoke;
            return clone;
        }
        public override void UpdateLinkedElement(bool updateChildren = true)
        {
            if (updateChildren)
            {
                ConditionBeforeInvokeData?.UpdateLinkedElement();
                ConditionAfterInvokeData?.UpdateLinkedElement();
            }
            base.UpdateLinkedElement(updateChildren);
        }
        public override void RegisterLinkedElement()
        {
            ConditionBeforeInvokeData?.RegisterLinkedElement();
            ConditionAfterInvokeData?.RegisterLinkedElement();
            base.RegisterLinkedElement();
        }
    }
}
