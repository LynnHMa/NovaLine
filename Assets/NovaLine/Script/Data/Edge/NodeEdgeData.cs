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
        public override string describtion => "Set next node and its condition.";

        public NodeEdgeData() { }
        public NodeEdgeData(NodeSwitcher novaSwitcher) : base(novaSwitcher)
        {
            switchConditionData = new ConditionData(novaSwitcher.switchCondition);
        }

        public override void init(NovaSwitcher linkedSwitcher)
        {
            base.init(linkedSwitcher);
            if (linkedSwitcher is not NodeSwitcher nodeSwitcher) return;
            switchConditionData = new ConditionData(nodeSwitcher.switchCondition);
        }
        
        public override void registerLinkedElement()
        {
            if (switchConditionData != null)
            {
                var conditionElement = switchConditionData.linkedElement;
                if (conditionElement != null)
                {
                    NovaElementRegistry.RegisterElement(conditionElement);
                }
            }
            base.registerLinkedElement();
        }
        
        public override void updateLinkedElement()
        {
            base.updateLinkedElement();
            if (linkedSwitcher != null && switchConditionData != null)
            {
                linkedSwitcher.switchConditionGuid = switchConditionData.guid;
            }
        }
        public override INovaData copy()
        {
            if (base.copy() is not NodeEdgeData clone) return null;
            clone.switchConditionData = (ConditionData)switchConditionData.copy();
            clone.switchConditionData.linkedElement = clone.linkedSwitcher.switchCondition;
            return clone;
        }
    }
}
