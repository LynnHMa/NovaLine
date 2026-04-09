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
        
        public virtual List<TChildGraphViewNodeData> nodeDataList
        {
            get => _nodeDataList;
            set => _nodeDataList = value;
        }
        public virtual List<TChildEdgeData> edgeDataList
        {
            get => _edgeDataList;
            set => _edgeDataList = value;
        }
        public override string name => linkedElement?.name;
        public override string description => linkedElement?.describtion;
        public override string guid => linkedElement?.guid;
        
        List<IGraphViewNodeData> IGraphViewNodeData.nodeDataList
        {
            get => nodeDataList.Cast<IGraphViewNodeData>().ToList();
            set => nodeDataList = value.Cast<TChildGraphViewNodeData>().ToList();
        }
        List<IEdgeData> IGraphViewNodeData.edgeDataList
        {
            get => edgeDataList.Cast<IEdgeData>().ToList();
            set => edgeDataList = value.Cast<TChildEdgeData>().ToList();
        }

        public override void registerLinkedElement()
        {
            if (linkedElement == null) return;
            foreach (var nodeGraphViewData in nodeDataList)
            {
                nodeGraphViewData.linkedElement.setParent(linkedElement);
                nodeGraphViewData.registerLinkedElement();
            }
            foreach (var edgeData in edgeDataList)
            {
                var switcher = edgeData.linkedElement;
                var outputElement = NovaElementRegistry.FindElement(switcher?.outputElementGuid);
                outputElement?.onGraphConnect(switcher);
                switcher?.setParent(linkedElement);
                edgeData.registerLinkedElement();
            }

            NovaElementRegistry.RegisterElement(linkedElement);
        }

        public override void updateLinkedElement(bool updateChildren = true)
        {
            if (updateChildren)
            {
                foreach (var graphNodeData in nodeDataList)
                {
                    graphNodeData?.updateLinkedElement();
                }
                foreach (var edgeData in edgeDataList)
                {
                    edgeData?.updateLinkedElement();
                }
            }
            linkedElement = NovaElementRegistry.FindElement(linkedElement?.guid) as TLinkedElement;
        }
        public override INovaData copy()
        {
            try
            {
                var clone = (IGraphViewNodeData)base.copy();
                if (clone == null) return null;
                
                clone.edgeDataList.Clear();
                clone.nodeDataList.Clear();
                clone.linkedElement = null;
                
                var firstNodeDataIndex = -1;
                
                if (linkedElement == null) return null;
                
                if (linkedElement.firstChild != null)
                    firstNodeDataIndex = nodeDataList.FindIndex(n => n.guid.Equals(linkedElement.firstChild.guid));
                
                var guidRemap = new Dictionary<string, string>();
                if (nodeDataList.Count > 0)
                {
                    var nodeDataListClone = new List<IGraphViewNodeData>();
                    for (var i = 0; i < nodeDataList.Count; i++)
                    {
                        var originalNodeData = nodeDataList[i];
                        var clonedNodeData   = (TChildGraphViewNodeData)originalNodeData.copy();
                        guidRemap[originalNodeData.guid] = clonedNodeData.guid;
                        nodeDataListClone.Add(clonedNodeData);
                    }
                    clone.nodeDataList = nodeDataListClone;
                }
                
                if (edgeDataList.Count > 0)
                {
                    var edgeDataListClone = new List<IEdgeData>();
                    for (var i = 0; i < edgeDataList.Count; i++)
                    {
                        var originalEdgeData = edgeDataList[i];
                        var clonedEdgeData   = (TChildEdgeData)originalEdgeData.copy();
                        
                        var originalSwitcher = originalEdgeData.linkedElement;
                        var clonedSwitcher   = clonedEdgeData.linkedElement;

                        if (originalSwitcher.inputElementGuid != null &&
                            guidRemap.TryGetValue(originalSwitcher.inputElementGuid, out var newIn))
                            clonedSwitcher.inputElementGuid = newIn;

                        if (originalSwitcher.outputElementGuid != null &&
                            guidRemap.TryGetValue(originalSwitcher.outputElementGuid, out var newOut))
                            clonedSwitcher.outputElementGuid = newOut;

                        edgeDataListClone.Add(clonedEdgeData);
                    }
                    clone.edgeDataList = edgeDataListClone;
                }

                clone.linkedElement = linkedElement.copy();
                if (nodeDataList.Count > 0 && firstNodeDataIndex >= 0)
                    clone.linkedElement.firstChildGuid = clone.nodeDataList[firstNodeDataIndex].linkedElement.guid;

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
        List<IGraphViewNodeData> nodeDataList { get; set; }
        List<IEdgeData> edgeDataList { get; set; }
    }
}
