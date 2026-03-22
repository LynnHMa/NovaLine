using System;
using System.Collections.Generic;

namespace NovaLine.Data.NodeGraphView
{
    using NovaLine.Element;
    using UnityEngine;
    using NovaLine.Data.Edge;

    [Serializable]
    public class NodeData : GraphViewNodeData<Node, ActionData, ActionEdgeData>
    {
        [SerializeReference] private Node _linkedElement;
        [SerializeReference] private List<ActionData> _nodeDatas = new();
        [SerializeReference] private List<ActionEdgeData> _edgeDatas = new();
        [SerializeReference] private ConditionData _conditionBeforeInvokeData;
        [SerializeReference] private ConditionData _conditionAfterInvokeData;

        public override Node linkedElement
        {
            get => _linkedElement;
            set => _linkedElement = value;
        }
        public override List<ActionData> nodeDatas
        {
            get => _nodeDatas != null ? _nodeDatas : new();
            set => _nodeDatas = value != null ? value : _nodeDatas;
        }
        public override List<ActionEdgeData> edgeDatas
        {
            get => _edgeDatas != null ? _edgeDatas : new();
            set => _edgeDatas = value != null ? value : _edgeDatas;
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
        public override string startGraphNodeGuid
        {
            get => linkedElement?.firstAction?.guid ?? base.startGraphNodeGuid;
            set => base.startGraphNodeGuid = value;
        }

        public NodeData(Node node,Vector2 pos)
        {
            this.pos = pos;
            linkedElement = node;
            startGraphNodeGuid = linkedElement.firstAction?.guid;
            conditionBeforeInvokeData = new ConditionData(linkedElement?.conditionBeforeInvoke);
            conditionAfterInvokeData = new ConditionData(linkedElement?.conditionAfterInvoke);
        }
    }
}
