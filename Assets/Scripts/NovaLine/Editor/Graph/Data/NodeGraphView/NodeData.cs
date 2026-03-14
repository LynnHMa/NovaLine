using System;
using System.Collections.Generic;

namespace NovaLine.Editor.Graph.Data.NodeGraphView
{
    using NovaLine.Element;
    using NovaLine.Editor.Graph.View;
    using UnityEngine;
    using NovaLine.Utils;
    using NovaLine.Editor.Graph.Data.Edge;

    [Serializable]
    public class NodeData : GraphViewNodeData<Node,ActionData,ActionEdgeData>
    {
        [SerializeReference] private Node               _linkedElement;
        [SerializeReference] private List<ActionData>      _nodeDatas  = new();
        [SerializeReference] private List<ActionEdgeData>  _edgeDatas  = new();
        [SerializeReference] private ConditionData _conditionBeforeInvokeData;
        [SerializeReference] private ConditionData _conditionAfterInvokeData;

        public override Node linkedElement
        {
            get => _linkedElement;
            set => _linkedElement = value;
        }
        public override EList<ActionData> nodeDatas
        {
            get => _nodeDatas != null ? new(_nodeDatas) : new();
            set => _nodeDatas = value != null ? new(value) : new();
        }
        public override EList<ActionEdgeData> edgeDatas
        {
            get => _edgeDatas != null ? new(_edgeDatas) : new();
            set => _edgeDatas = value != null ? new(value) : new();
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

        public NodeData(NodeGraphView nodeGraphView, Vector2 pos)
        {
            this.pos = pos;
            linkedElement = nodeGraphView.root;
            startGraphNodeGuid = linkedElement.firstAction?.guid;
            conditionBeforeInvokeData = new ConditionData(linkedElement?.conditionBeforeInvoke);
            conditionAfterInvokeData = new ConditionData(linkedElement?.conditionAfterInvoke);
        }
    }
}
