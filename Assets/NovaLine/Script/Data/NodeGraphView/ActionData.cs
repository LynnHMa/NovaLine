using System;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Data.NodeGraphView
{
    using NovaLine.Script.Action;
    using UnityEngine;

    [Serializable]
    public class ActionData : GraphViewNodeData<NovaAction, IObject, IObject>,IAroundConditionData
    {
        [SerializeReference,HideInInspector] private ConditionData _conditionBeforeInvokeData;
        [SerializeReference,HideInInspector] private ConditionData _conditionAfterInvokeData;
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
            Pos = pos;
            LinkedElement = linkedAction;
            ConditionBeforeInvokeData = new ConditionData(LinkedElement?.ConditionBeforeInvoke);
            ConditionAfterInvokeData = new ConditionData(LinkedElement?.ConditionAfterInvoke);
        }
        
        public override INovaData Copy()
        {
            var actionData = (ActionData)base.Copy();
            
            if (actionData == null) return null;
            
            if (ConditionBeforeInvokeData != null && ConditionAfterInvokeData != null)
            {
                actionData.ConditionBeforeInvokeData = (ConditionData)ConditionBeforeInvokeData.Copy();
                actionData.ConditionAfterInvokeData  = (ConditionData)ConditionAfterInvokeData.Copy();
                if (actionData.LinkedElement is IAroundConditionElement condElement)
                {
                    condElement.ConditionBeforeInvokeGuid = actionData.ConditionBeforeInvokeData.LinkedElement?.Guid;
                    condElement.ConditionAfterInvokeGuid  = actionData.ConditionAfterInvokeData.LinkedElement?.Guid;
                    actionData.ConditionBeforeInvokeData.LinkedElement?.SetParent(actionData.LinkedElement); 
                    actionData.ConditionAfterInvokeData.LinkedElement?.SetParent(actionData.LinkedElement); 
                }
            }
            
            return actionData;
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
            ConditionBeforeInvokeData?.LinkedElement?.SetParent(LinkedElement);
            ConditionAfterInvokeData?.LinkedElement?.SetParent(LinkedElement);
            ConditionBeforeInvokeData?.RegisterLinkedElement();
            ConditionAfterInvokeData?.RegisterLinkedElement();
            base.RegisterLinkedElement();
        }

        public override void UnregisterLinkedElement()
        {
            ConditionBeforeInvokeData?.UnregisterLinkedElement();
            ConditionAfterInvokeData?.UnregisterLinkedElement();
            base.UnregisterLinkedElement();
        }
    }
}
