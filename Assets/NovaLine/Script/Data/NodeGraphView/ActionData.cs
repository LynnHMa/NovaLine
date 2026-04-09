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

        public ActionData(){}
        public ActionData(NovaAction linkedAction, Vector2 pos)
        {
            this.pos = pos;
            linkedElement = linkedAction;
            conditionBeforeInvokeData = new ConditionData(linkedElement?.conditionBeforeInvoke);
            conditionAfterInvokeData = new ConditionData(linkedElement?.conditionAfterInvoke);
        }
        
        public override INovaData copy()
        {
            var clone = (ActionData)base.copy();
            
            if (clone == null) return null;
            
            clone.conditionBeforeInvokeData = (ConditionData)conditionBeforeInvokeData.copy();
            clone.conditionAfterInvokeData = (ConditionData)conditionAfterInvokeData.copy();
            clone.conditionBeforeInvokeData.linkedElement = clone.linkedElement.conditionBeforeInvoke;
            clone.conditionAfterInvokeData.linkedElement = clone.linkedElement.conditionAfterInvoke;
            return clone;
        }

        public override void updateLinkedElement(bool updateChildren = true)
        {
            if (updateChildren)
            {
                conditionBeforeInvokeData?.updateLinkedElement();
                conditionAfterInvokeData?.updateLinkedElement();
            }
            base.updateLinkedElement(updateChildren);
        }
        public override void registerLinkedElement()
        {
            conditionBeforeInvokeData?.registerLinkedElement();
            conditionAfterInvokeData?.registerLinkedElement();
            base.registerLinkedElement();
        }
    }
}
