﻿﻿﻿using NovaLine.Script.Data.Edge;
using NovaLine.Script.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace NovaLine.Script.Data.NodeGraphView
{
    [Serializable]
    public abstract class GraphViewNodeData<TLinkedElement,TChildGraphViewNodeData, TChildEdgeData> : NovaData<TLinkedElement>,IGraphViewNodeData 
        where TLinkedElement : NovaElement where TChildGraphViewNodeData : IGraphViewNodeData where TChildEdgeData : IEdgeData
    {
        [SerializeReference] private List<TChildGraphViewNodeData> _nodeDataList = new();
        [SerializeReference] private List<TChildEdgeData> _edgeDataList = new();
        
        public virtual List<TChildGraphViewNodeData> NodeDataList
        {
            get => _nodeDataList;
            set => _nodeDataList = value;
        }
        public virtual List<TChildEdgeData> EdgeDataList
        {
            get => _edgeDataList;
            set => _edgeDataList = value;
        }
        public override string Name => LinkedElement?.name;
        public override string Description => LinkedElement?.description;
        public override string Guid => LinkedElement?.Guid;
        
        List<IGraphViewNodeData> IGraphViewNodeData.NodeDataList
        {
            get => NodeDataList.Cast<IGraphViewNodeData>().ToList();
            set => NodeDataList = value.Cast<TChildGraphViewNodeData>().ToList();
        }
        List<IEdgeData> IGraphViewNodeData.EdgeDataList
        {
            get => EdgeDataList.Cast<IEdgeData>().ToList();
            set => EdgeDataList = value.Cast<TChildEdgeData>().ToList();
        }

        public override void RegisterLinkedElement()
        {
            if (LinkedElement == null) return;
            foreach (var nodeGraphViewData in NodeDataList)
            {
                nodeGraphViewData.LinkedElement.SetParent(LinkedElement);
                nodeGraphViewData.RegisterLinkedElement();
            }
            foreach (var edgeData in EdgeDataList)
            {
                var switcher = edgeData.LinkedElement;
                var outputElement = NovaElementRegistry.FindElement(switcher?.outputElementGuid);
                outputElement?.onGraphConnect(switcher);
                switcher?.SetParent(LinkedElement);
                edgeData.RegisterLinkedElement();
            }

            NovaElementRegistry.RegisterElement(LinkedElement);
        }

        public override void UpdateLinkedElement(bool updateChildren = true)
        {
            if (updateChildren)
            {
                foreach (var graphNodeData in NodeDataList)
                {
                    graphNodeData?.UpdateLinkedElement();
                }
                foreach (var edgeData in EdgeDataList)
                {
                    edgeData?.UpdateLinkedElement();
                }
            }
            LinkedElement = NovaElementRegistry.FindElement(LinkedElement?.Guid) as TLinkedElement;
        }
        public override INovaData Copy()
        {
            try
            {
                var clone = (IGraphViewNodeData)base.Copy();
                if (clone == null) return null;
                
                clone.EdgeDataList.Clear();
                clone.NodeDataList.Clear();
                clone.LinkedElement = null;
                
                var firstNodeDataIndex = -1;
                
                if (LinkedElement == null) return null;
                
                if (LinkedElement.FirstChild != null)
                    firstNodeDataIndex = NodeDataList.FindIndex(n => n.Guid.Equals(LinkedElement.FirstChild.Guid));
                
                var guidRemap = new Dictionary<string, string>();
                if (NodeDataList.Count > 0)
                {
                    var nodeDataListClone = new List<IGraphViewNodeData>();
                    for (var i = 0; i < NodeDataList.Count; i++)
                    {
                        var originalNodeData = NodeDataList[i];
                        var clonedNodeData   = (TChildGraphViewNodeData)originalNodeData.Copy();
                        guidRemap[originalNodeData.Guid] = clonedNodeData.Guid;
                        nodeDataListClone.Add(clonedNodeData);
                    }
                    clone.NodeDataList = nodeDataListClone;
                }
                
                if (EdgeDataList.Count > 0)
                {
                    var edgeDataListClone = new List<IEdgeData>();
                    for (var i = 0; i < EdgeDataList.Count; i++)
                    {
                        var originalEdgeData = EdgeDataList[i];
                        var clonedEdgeData   = (TChildEdgeData)originalEdgeData.Copy();
                        
                        var originalSwitcher = originalEdgeData.LinkedElement;
                        var clonedSwitcher   = clonedEdgeData.LinkedElement;

                        if (originalSwitcher.inputElementGuid != null &&
                            guidRemap.TryGetValue(originalSwitcher.inputElementGuid, out var newIn))
                            clonedSwitcher.inputElementGuid = newIn;

                        if (originalSwitcher.outputElementGuid != null &&
                            guidRemap.TryGetValue(originalSwitcher.outputElementGuid, out var newOut))
                            clonedSwitcher.outputElementGuid = newOut;

                        edgeDataListClone.Add(clonedEdgeData);
                    }
                    clone.EdgeDataList = edgeDataListClone;
                }

                clone.LinkedElement = LinkedElement.Copy();
                if (NodeDataList.Count > 0 && firstNodeDataIndex >= 0)
                    clone.LinkedElement.FirstChildGuid = clone.NodeDataList[firstNodeDataIndex].LinkedElement.Guid;

                return clone;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
    }
    public interface IGraphViewNodeData : INovaData
    {
        List<IGraphViewNodeData> NodeDataList { get; set; }
        List<IEdgeData> EdgeDataList { get; set; }
    }
}
