using NovaLine.Data.Edge;
using NovaLine.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace NovaLine.Data.NodeGraphView
{
    [Serializable]
    public abstract class GraphViewNodeData<T, CN, CE> : NovaData, IGraphViewNodeData where T : NovaElement where CN : IGraphViewNodeData where CE : IEdgeData
    {
        [SerializeReference] private T _linkedElement;
        [SerializeReference] private List<CN> _nodeDatas = new();
        [SerializeReference] private List<CE> _edgeDatas = new();
        
        public override string name => linkedElement?.name;
        public override string describtion => linkedElement?.describtion;
        public override string guid => linkedElement?.guid;
        public virtual NovaElementType type => linkedElement != null ? linkedElement.type : NovaElementType.NONE;
        public virtual string startGraphNodeGuid => linkedElement?.firstChild?.guid;
        public virtual T linkedElement 
        { 
            get => _linkedElement; 
            set => _linkedElement = value; 
        }
        public virtual List<CN> nodeDatas
        {
            get => _nodeDatas;
            set => _nodeDatas = value;
        }

        public virtual List<CE> edgeDatas
        {
            get => _edgeDatas;
            set => _edgeDatas = value;
        }

        NovaElement IGraphViewNodeData.linkedElement { get => linkedElement; set => linkedElement = value as T; }

        List<IGraphViewNodeData> IGraphViewNodeData.nodeDatas
        {
            get => nodeDatas.Cast<IGraphViewNodeData>().ToList();
            set => nodeDatas = value.Cast<CN>().ToList();
        }

        List<IEdgeData> IGraphViewNodeData.edgeDatas
        {
            get => edgeDatas.Cast<IEdgeData>().ToList();
            set => edgeDatas = value.Cast<CE>().ToList();
        }

        public override INovaData copy()
        {
            try
            {
                var clone = (IGraphViewNodeData)base.copy();

                if (clone == null) return null;
                
                clone.edgeDatas.Clear();
                clone.nodeDatas.Clear();
                clone.linkedElement = null;
                
                var firstNodeDataIndex = -1;
                if(_linkedElement.firstChild != null) firstNodeDataIndex = nodeDatas.FindIndex(nodeData => nodeData.guid.Equals(_linkedElement.firstChild.guid));

                if (nodeDatas.Count > 0)
                {
                    var nodeDatasClone = new List<IGraphViewNodeData>();
                    for (var i = nodeDatas.Count - 1; i >= 0; i--)
                    {
                        var nodeData = nodeDatas[i];
                        nodeDatasClone.Add((CN)nodeData.copy());
                    }
                    clone.nodeDatas = nodeDatasClone;
                }

                if (edgeDatas.Count > 0)
                {
                    var edgeDatasClone = new List<IEdgeData>();
                    for (var i = edgeDatas.Count - 1; i >= 0; i--)
                    {
                        var edgeData = edgeDatas[i];
                        edgeDatasClone.Add((CE)edgeData.copy());
                    }
                    clone.edgeDatas = edgeDatasClone;
                }

                clone.linkedElement = linkedElement.copy();
                if(nodeDatas.Count > 0 && firstNodeDataIndex >= 0) clone.linkedElement.firstChild = clone.nodeDatas[firstNodeDataIndex].linkedElement;
                return clone;
            }
            catch (Exception e)
            {
                Debug.Log("Error in copy graph view node data! " + e.Message);
                Debug.Log(e.StackTrace);
                return null;
            }
        }
    }
    public interface IGraphViewNodeData : INovaData
    {
        public NovaElement linkedElement { get; set; }
        public List<IGraphViewNodeData> nodeDatas { get; set; }
        public List<IEdgeData> edgeDatas { get; set; }
        public string startGraphNodeGuid { get; }
    }
}
