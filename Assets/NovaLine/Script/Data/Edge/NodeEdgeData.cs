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

        public override string name => "Next Node";
        public override string description => "Set next node and its condition.";

        public NodeEdgeData() { }
        public NodeEdgeData(NodeSwitcher novaElement) : base(novaElement)
        {
            switchConditionData = new ConditionData(novaElement.switchCondition);
        }
        
        public override void registerLinkedElement()
        {
            switchConditionData?.registerLinkedElement();
            base.registerLinkedElement();
        }
        
        public override void updateLinkedElement(bool updateChildren = true)
        {
            if(updateChildren) switchConditionData?.updateLinkedElement();
            base.updateLinkedElement(updateChildren);
        }
        public override INovaData copy()
        {
            if (base.copy() is not NodeEdgeData clone) return null;
            clone.switchConditionData = (ConditionData)switchConditionData.copy();
            clone.switchConditionData.linkedElement = clone.linkedElement.switchCondition;
            return clone;
        }
    }
}
