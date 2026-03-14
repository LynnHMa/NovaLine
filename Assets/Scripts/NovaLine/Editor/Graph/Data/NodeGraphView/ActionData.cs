using System;

namespace NovaLine.Editor.Graph.Data.NodeGraphView
{
    using NovaLine.Action;
    using NovaLine.Editor.Utils.Interface;
    using UnityEngine;

    [Serializable]
    public class ActionData : GraphViewNodeData<NovaAction, IObject, IObject>
    {
        [SerializeReference] private NovaAction _linkedElement;
        [SerializeReference] private ConditionData _conditionBeforeInvokeData;
        [SerializeReference] private ConditionData _conditionAfterInvokeData;
        public override NovaAction linkedElement
        {
            get => _linkedElement;
            set => _linkedElement = value;
        }
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

        public ActionData(NovaAction linkedAction, Vector2 pos)
        {
            this.pos = pos;
            linkedElement = linkedAction;
            conditionBeforeInvokeData = new ConditionData(linkedElement?.conditionBeforeInvoke);
            conditionAfterInvokeData = new ConditionData(linkedElement?.conditionAfterInvoke);
        }
    }
}
