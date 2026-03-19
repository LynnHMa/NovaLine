using NovaLine.Element;
using System;
using System.Collections.Generic;
using UnityEngine;
using NovaLine.Utils.Ext;
using NovaLine.Editor.Graph.Data.Edge;

namespace NovaLine.Editor.Graph.Data.NodeGraphView
{
    [Serializable]
    public class FlowchartData : GraphViewNodeData<Flowchart,NodeData,NodeEdgeData>
    {

        [SerializeReference] private Flowchart           _linkedElement;
        [SerializeReference] private List<NodeData>      _nodeDatas    = new();
        [SerializeReference] private List<NodeEdgeData>  _edgeDatas    = new();

        public override Flowchart linkedElement
        {
            get => _linkedElement;
            set => _linkedElement = value;
        }
        public override List<NodeData> nodeDatas
        {
            get => _nodeDatas != null ? new(_nodeDatas) : new();
            set => _nodeDatas = value != null ? new(value) : new();
        }
        public override List<NodeEdgeData> edgeDatas
        {
            get => _edgeDatas != null ? new(_edgeDatas) : new();
            set => _edgeDatas = value != null ? new(value) : new();
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
