using NovaLine.Editor.Graph.Data.NodeGraphView;
using NovaLine.Switcher;
using System;
using UnityEngine;

namespace NovaLine.Editor.Graph.Data.Edge
{
    [Serializable]
    public class NodeEdgeData : EdgeData<Element.Node, NodeSwitcher>
    {
        [SerializeReference] private NodeSwitcher _linkedSwitcher;
        [SerializeReference] private ConditionData _switchConditionData;

        public override NodeSwitcher linkedSwitcher
        {
            get => _linkedSwitcher;
            set => _linkedSwitcher = value;
        }
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

        public override void onSummon(NovaSwitcher linkedSwitcher)
        {
            base.onSummon(linkedSwitcher);
            if (linkedSwitcher is not NodeSwitcher nodeSwitcher) return;
            switchConditionData = new ConditionData(nodeSwitcher.switchCondition);
        }
    }
}
