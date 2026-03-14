using NovaLine.Editor.Graph.Data.Edge;
using NovaLine.Editor.Graph.View;
using NovaLine.Element;
using NovaLine.Utils;
using System;
using System.Linq;
using UnityEngine;
namespace NovaLine.Editor.Graph.Data.NodeGraphView
{
    [Serializable]
    public class GraphViewNodeData<T,CN,CE> : NovaData, IGraphViewNodeData where T : NovaElement where CN : IGraphViewNodeData where CE : IEdgeData
    {
        public override string name => linkedElement?.name;
        public override string describtion => linkedElement?.describtion;
        public override string guid => linkedElement?.guid;

        [SerializeField] private string _startGraphNodeGuid;
        public virtual string startGraphNodeGuid { get => _startGraphNodeGuid; set => _startGraphNodeGuid = value; }

        public virtual T linkedElement { get; set; }
        public virtual EList<CN> nodeDatas { get; set; } = new();
        public virtual EList<CE> edgeDatas { get; set; } = new();

        NovaElement IGraphViewNodeData.linkedElement { get => linkedElement; set => linkedElement = value as T; }
        EList<IGraphViewNodeData> IGraphViewNodeData.nodeDatas { get => nodeDatas.Cast<IGraphViewNodeData>().ToEList(); set => nodeDatas = value.Cast<CN>().ToEList(); }
        EList<IEdgeData> IGraphViewNodeData.edgeDatas { get => edgeDatas.Cast<IEdgeData>().ToEList(); set => edgeDatas = value.Cast<CE>().ToEList(); }
    }
    public interface IGraphViewNodeData : INovaData
    {
        public NovaElement linkedElement { get; set; }
        public EList<IGraphViewNodeData> nodeDatas { get; set; }
        public EList<IEdgeData> edgeDatas { get; set; }
        public string startGraphNodeGuid { get; set; }
        public virtual void draw(INovaGraphView graphView) { }
    }
}
