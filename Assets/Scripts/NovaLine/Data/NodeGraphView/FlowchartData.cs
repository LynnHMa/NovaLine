using NovaLine.Data.Edge;
using NovaLine.Element;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovaLine.Data.NodeGraphView
{
    [Serializable]
    public class FlowchartData : GraphViewNodeData<Flowchart, NodeData, NodeEdgeData>
    {

        [SerializeReference] private Flowchart _linkedElement;
        [SerializeReference] private List<NodeData> _nodeDatas = new();
        [SerializeReference] private List<NodeEdgeData> _edgeDatas = new();

        public override Flowchart linkedElement
        {
            get => _linkedElement;
            set => _linkedElement = value;
        }
        public override List<NodeData> nodeDatas
        {
            get => _nodeDatas != null ? _nodeDatas : new();
            set => _nodeDatas = value != null ? value : _nodeDatas;
        }
        public override List<NodeEdgeData> edgeDatas
        {
            get => _edgeDatas != null ? _edgeDatas : new();
            set => _edgeDatas = value != null ? value : _edgeDatas;
        }
        public override string startGraphNodeGuid
        {
            get => linkedElement?.firstNode?.guid ?? base.startGraphNodeGuid;
            set => base.startGraphNodeGuid = value;
        }

        public FlowchartData()
        {
            guid = Guid.NewGuid().ToString();
            linkedElement = new Flowchart("New Flowchart");
        }
    }
}
