﻿﻿using NovaLine.Script.Data.Edge;
using NovaLine.Script.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace NovaLine.Script.Data.NodeGraphView
{
    [Serializable]
    public abstract class GraphViewNodeData<T, CN, CE> : NovaData, IGraphViewNodeData where T : NovaElement where CN : IGraphViewNodeData where CE : IEdgeData
    {
        [SerializeReference] private T _linkedElement;
        [SerializeReference] private List<CN> _nodeDataList = new();
        [SerializeReference] private List<CE> _edgeDataList = new();
        
        public virtual T linkedElement 
        { 
            get => _linkedElement; 
            set => _linkedElement = value; 
        }
        public virtual List<CN> nodeDataList
        {
            get => _nodeDataList;
            set => _nodeDataList = value;
        }
        public virtual List<CE> edgeDataList
        {
            get => _edgeDataList;
            set => _edgeDataList = value;
        }
        public virtual NovaElementType type => linkedElement?.type ?? NovaElementType.NONE;
        public virtual string startGraphNodeGuid => linkedElement?.firstChildGuid;
        public override string name => linkedElement?.name;
        public override string describtion => linkedElement?.describtion;
        public override string guid => linkedElement?.guid;

        NovaElement IGraphViewNodeData.linkedElement
        {
            get => linkedElement; 
            set => linkedElement = value as T;
        }
        List<IGraphViewNodeData> IGraphViewNodeData.nodeDataList
        {
            get => nodeDataList.Cast<IGraphViewNodeData>().ToList();
            set => nodeDataList = value.Cast<CN>().ToList();
        }
        List<IEdgeData> IGraphViewNodeData.edgeDataList
        {
            get => edgeDataList.Cast<IEdgeData>().ToList();
            set => edgeDataList = value.Cast<CE>().ToList();
        }

        public virtual void registerLinkedElement()
        {
            if (linkedElement == null) return;
            foreach (var nodeGraphViewData in nodeDataList)
            {
                nodeGraphViewData.linkedElement.setParent(linkedElement);
                nodeGraphViewData.registerLinkedElement();
            }
            foreach (var edgeData in edgeDataList)
            {
                var switcher = edgeData.linkedSwitcher;
                var outputElement = NovaElementRegistry.FindElement(switcher?.outputElementGuid);
                outputElement?.onGraphConnect(switcher);
                switcher?.setParent(linkedElement);
                edgeData.registerLinkedElement();
            }

            NovaElementRegistry.RegisterElement(linkedElement);
        }

        public virtual void updateLinkedElement(bool updateChildren = true)
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
            linkedElement = NovaElementRegistry.FindElement(_linkedElement.guid) as T;
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
                if (_linkedElement.firstChild != null)
                    firstNodeDataIndex = nodeDataList.FindIndex(n => n.guid.Equals(_linkedElement.firstChild.guid));
                
                var guidRemap = new Dictionary<string, string>();
                if (nodeDataList.Count > 0)
                {
                    var nodeDataListClone = new List<IGraphViewNodeData>();
                    for (var i = 0; i < nodeDataList.Count; i++)
                    {
                        var originalNodeData = nodeDataList[i];
                        var clonedNodeData   = (CN)originalNodeData.copy();
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
                        var clonedEdgeData   = (CE)originalEdgeData.copy();
                        
                        var originalSwitcher = originalEdgeData.linkedSwitcher;
                        var clonedSwitcher   = clonedEdgeData.linkedSwitcher;

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
        public NovaElement linkedElement { get; set; }
        public List<IGraphViewNodeData> nodeDataList { get; set; }
        public List<IEdgeData> edgeDataList { get; set; }
        public string startGraphNodeGuid { get; }
        void registerLinkedElement();
        void updateLinkedElement(bool updateChildren = true);
    }
}
