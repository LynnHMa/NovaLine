using NovaLine.Element;
using NovaLine.Editor.Graph.View;
using System;
using System.Collections.Generic;
using UnityEngine;
using NovaLine.Utils;
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
        public override EList<NodeData> nodeDatas
        {
            get => _nodeDatas != null ? new(_nodeDatas) : new();
            set => _nodeDatas = value != null ? new(value) : new();
        }
        public override EList<NodeEdgeData> edgeDatas
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
    [CreateAssetMenu]
    public class FlowchartDataAsset : ScriptableObject
    {
        public FlowchartData data;

        public static FlowchartDataAsset CreateInstance(FlowchartData data = null)
        {
            var result = CreateInstance<FlowchartDataAsset>();
            result.data = data == null ? new FlowchartData() : data;
            return result;
        }
    }
}
