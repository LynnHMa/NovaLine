using NovaLine.Script.Data.NodeGraphView;
using System;
using NovaLine.Script.Element.Switcher;
using UnityEngine;

namespace NovaLine.Script.Data.Edge
{
    [Serializable]
    public class NodeEdgeData : EdgeData<NodeSwitcher>
    {
        [SerializeReference,HideInInspector] private ConditionData _switchConditionData;
        public ConditionData SwitchConditionData
        {
            get => _switchConditionData;
            set => _switchConditionData = value;
        }

        public override string Name => "Next Node";
        public override string Description => "Set next node and its condition.";

        public NodeEdgeData() { }
        public NodeEdgeData(NodeSwitcher novaElement) : base(novaElement)
        {
            SwitchConditionData = new ConditionData(novaElement.SwitchCondition);
        }
        
        public override void RegisterLinkedElement()
        {
            SwitchConditionData?.LinkedElement?.SetParent(LinkedElement);
            SwitchConditionData?.RegisterLinkedElement();
            base.RegisterLinkedElement();
        }

        public override void UnregisterLinkedElement()
        {
            SwitchConditionData?.UnregisterLinkedElement();
            base.UnregisterLinkedElement();
        }
        
        public override void UpdateLinkedElement(bool updateChildren = true)
        {
            if(updateChildren) SwitchConditionData?.UpdateLinkedElement();
            base.UpdateLinkedElement(updateChildren);
        }
        public override INovaData Copy()
        {
            if (base.Copy() is not NodeEdgeData nodeEdgeData) return null;
            
            if (SwitchConditionData != null)
            {
                nodeEdgeData.SwitchConditionData = (ConditionData)SwitchConditionData.Copy();
                nodeEdgeData.LinkedElement.SwitchConditionGuid = nodeEdgeData.SwitchConditionData.LinkedElement?.Guid;
                nodeEdgeData.SwitchConditionData.LinkedElement?.SetParent(nodeEdgeData.LinkedElement); 
            }
            
            return nodeEdgeData;
        }
    }
}
