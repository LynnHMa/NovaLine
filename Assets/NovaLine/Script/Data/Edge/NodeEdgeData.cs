using NovaLine.Script.Data.NodeGraphView;
using System;
using NovaLine.Script.Element.Switcher;
using UnityEngine;

namespace NovaLine.Script.Data.Edge
{
    [Serializable]
    public class NodeEdgeData : EdgeData<NodeSwitcher>
    {
        [SerializeReference] private ConditionData _switchConditionData;
        public ConditionData switchConditionData
        {
            get => _switchConditionData;
            set => _switchConditionData = value;
        }

        public override string Name => "Next Node";
        public override string Description => "Set next node and its condition.";

        public NodeEdgeData() { }
        public NodeEdgeData(NodeSwitcher novaElement) : base(novaElement)
        {
            switchConditionData = new ConditionData(novaElement.switchCondition);
        }
        
        public override void RegisterLinkedElement()
        {
            switchConditionData?.RegisterLinkedElement();
            base.RegisterLinkedElement();
        }
        
        public override void UpdateLinkedElement(bool updateChildren = true)
        {
            if(updateChildren) switchConditionData?.UpdateLinkedElement();
            base.UpdateLinkedElement(updateChildren);
        }
        public override INovaData Copy()
        {
            if (base.Copy() is not NodeEdgeData clone) return null;
            clone.switchConditionData = (ConditionData)switchConditionData.Copy();
            clone.switchConditionData.LinkedElement = clone.LinkedElement.switchCondition;
            return clone;
        }
    }
}
