using System;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Data.NodeGraphView
{
    using NovaLine.Script.Action;
    using UnityEngine;

    [Serializable]
    public class ActionData : GraphViewNodeData<NovaAction, IObject, IObject>,IHasConditionData
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

        public ActionData(){}
        public ActionData(NovaAction linkedAction, Vector2 pos)
        {
            this.pos = pos;
            linkedElement = linkedAction;
            ConditionBeforeInvokeData = new ConditionData(linkedElement?.ConditionBeforeInvoke);
            ConditionAfterInvokeData = new ConditionData(linkedElement?.ConditionAfterInvoke);
        }
        
        public override INovaData copy()
        {
            var clone = (ActionData)base.copy();
            
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
