using NovaLine.Editor.Graph.Data.Edge;
using NovaLine.Editor.Graph.View;
using NovaLine.Element;
using NovaLine.Utils.Ext;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace NovaLine.Editor.Graph.Data.NodeGraphView
{
    [Serializable]
    public class GraphViewNodeData<T,CN,CE> : NovaData, IGraphViewNodeData where T : NovaElement where CN : IGraphViewNodeData where CE : IEdgeData
    {
        [SerializeReference] private string _startGraphNodeGuid;

        public override string name => linkedElement?.name;
        public override string describtion => linkedElement?.describtion;
        public override string guid => linkedElement?.guid;

        public virtual string startGraphNodeGuid { get => _startGraphNodeGuid; set => _startGraphNodeGuid = value; }
        public virtual T linkedElement { get; set; }
        public virtual List<CN> nodeDatas { get; set; } = new();
        public virtual List<CE> edgeDatas { get; set; } = new();

        NovaElement IGraphViewNodeData.linkedElement { get => linkedElement; set => linkedElement = value as T; }
        List<IGraphViewNodeData> IGraphViewNodeData.nodeDatas { get => nodeDatas.Cast<IGraphViewNodeData>().ToList(); set => nodeDatas = value.Cast<CN>().ToList(); }
        List<IEdgeData> IGraphViewNodeData.edgeDatas { get => edgeDatas.Cast<IEdgeData>().ToList(); set => edgeDatas = value.Cast<CE>().ToList(); }
    }
    public interface IGraphViewNodeData : INovaData
    {
        public NovaElement linkedElement { get; set; }
        public List<IGraphViewNodeData> nodeDatas { get; set; }
        public List<IEdgeData> edgeDatas { get; set; }
        public string startGraphNodeGuid { get; set; }
        public virtual void draw(INovaGraphView graphView) { }
    }
}
