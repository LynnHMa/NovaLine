﻿using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Utils.Ext;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window.Command;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static NovaLine.Script.Editor.Window.ContextRegistry;

namespace NovaLine.Script.Editor.Window
{
    public static class CopyPasteHelper
    {
        public static string Copy(IEnumerable<GraphElement> elements)
        {
            var copiedData = new CopyPasteData(elements);
            return JsonUtility.ToJson(copiedData);
        }

        public static void Paste(string operationName, string data)
        {
            if (!OnCanPaste(data)) return;
            using (new CommandScope())
            using (new SaveScope())
            {
                var copiedData = JsonUtility.FromJson<CopyPasteData>(data);
                if (GetContext(copiedData.linkedContextInfo.key,copiedData.linkedContextInfo.value) is not IGraphViewNodeContext parentContext) return;
                if (CurrentGraphViewNodeContext == null || CurrentGraphViewNodeContext.type != parentContext.type) return;
                var parentData = parentContext.linkedData;
                
                var currentGraphView = CurrentGraphViewNodeContext.graphView;
            
                if (parentData == null || currentGraphView == null) return;

                var copiedNodeGraphViewDataList = new List<IGraphViewNodeData>();
                var pastedNodeGraphViewDataList = new List<IGraphViewNodeData>();
                for (var i = 0; i < copiedData.nodeGraphViewDataGuids.Count; i++)
                {
                    var nodeGraphViewDataGuid = copiedData.nodeGraphViewDataGuids[i];
                    
                    var copiedNodeGraphViewData = getChildNodeGraphViewData(parentData, nodeGraphViewDataGuid);
                    var pastedNodeGraphViewData = (IGraphViewNodeData)copiedNodeGraphViewData?.copy();
                    if(copiedNodeGraphViewData == null || pastedNodeGraphViewData == null) continue;
                    
                    copiedNodeGraphViewDataList.Add(copiedNodeGraphViewData);
                    pastedNodeGraphViewDataList.Add(pastedNodeGraphViewData);
                    
                    var actualPos = currentGraphView.mousePos + pastedNodeGraphViewData.pos - copiedData.rootPos;

                    pastedNodeGraphViewData.pos = actualPos;
                    var newContext = currentGraphView.summonNewChildGraphViewNodeContext(pastedNodeGraphViewData);
                    newContext.linkedData = pastedNodeGraphViewData;
                    RegisterContext(newContext);
                    
                    CommandRegistry.Register(new AddNodeCommand(
                        currentGraphView.linkedElementGuid,
                        currentGraphView.linkedElement.type,
                        pastedNodeGraphViewData.strongCopy() as IGraphViewNodeData));
                    
                    pastedNodeGraphViewData.linkedElement.setParent(currentGraphView.linkedElement as NovaElement);
                    var newGraphNode = currentGraphView.summonNewGraphNode(pastedNodeGraphViewData.linkedElement, actualPos);
                    currentGraphView.addGraphNode(newGraphNode);
                }

                if (copiedNodeGraphViewDataList.Count == 0 || pastedNodeGraphViewDataList.Count == 0) return;
                
                for (var j = 0; j < copiedData.edgeDataGuids.Count; j++)
                {
                    var edgeDataGuid = copiedData.edgeDataGuids[j];
                    var copiedEdgeData = getChildEdgeData(parentData, edgeDataGuid);
                    if (copiedEdgeData == null) continue;
                    var pastedEdgeData = (IEdgeData)copiedEdgeData.copy();
                    if (pastedEdgeData == null) continue;

                    var inputElementIndex = copiedNodeGraphViewDataList.FindIndex(copiedNodeGraphViewData =>
                        copiedNodeGraphViewData.guid.Equals(copiedEdgeData.linkedElement.inputElementGuid));
                    var outputElementIndex = copiedNodeGraphViewDataList.FindIndex(copiedNodeGraphViewData =>
                        copiedNodeGraphViewData.guid.Equals(copiedEdgeData.linkedElement.outputElementGuid));
                    
                    if(inputElementIndex < 0 || inputElementIndex >= copiedNodeGraphViewDataList.Count || outputElementIndex < 0 || outputElementIndex >= copiedNodeGraphViewDataList.Count) continue;
                    
                    pastedEdgeData.linkedElement.inputElementGuid = pastedNodeGraphViewDataList[inputElementIndex]?.linkedElement?.guid;
                    pastedEdgeData.linkedElement.outputElementGuid = pastedNodeGraphViewDataList[outputElementIndex]?.linkedElement?.guid;

                    var newEdgeContext = currentGraphView.summonNewChildEdgeContext(pastedEdgeData);
                    RegisterContext(newEdgeContext);
                    pastedEdgeData.linkedElement.setParent(currentGraphView.linkedElement as NovaElement);
                    
                    CommandRegistry.Register(new AddEdgeCommand(
                        currentGraphView.linkedElementGuid,
                        currentGraphView.linkedElement.type,
                        pastedEdgeData.strongCopy() as IEdgeData));
                    
                    var newGraphEdge = currentGraphView.summonNewGraphEdge(pastedEdgeData.linkedElement);
                    currentGraphView.addGraphEdge(newGraphEdge);
                }
            }
        }
        
        public static bool OnCanPaste(string data)
        {
            return !String.IsNullOrEmpty(data);
        }

        private static IGraphViewNodeData getChildNodeGraphViewData(IGraphViewNodeData parentData,string guid)
        {
            return parentData.nodeDataList.Find(nodeData => nodeData.guid.Equals(guid));
        }

        private static IEdgeData getChildEdgeData(IGraphViewNodeData parentData, string guid)
        {
            return parentData.edgeDataList.Find(edgeData => edgeData.guid.Equals(guid));
        }
    }

    [Serializable]
    public class CopyPasteData
    {
        public KeyValue<string,NovaElementType> linkedContextInfo;
        public List<string> nodeGraphViewDataGuids = new();
        public List<string> edgeDataGuids = new();
        public Vector2 rootPos = Vector2.zero;
        
        public CopyPasteData(IEnumerable<GraphElement> selectedElements)
        {
            linkedContextInfo = new(CurrentGraphViewNodeContext.guid, CurrentGraphViewNodeContext.type);
            if (selectedElements == null || selectedElements.Count() == 0) return;

            var elements = selectedElements.OrderBy(e => e.GetPosition().y).ThenBy(e => e.GetPosition().x).ToList();
            var selectedNodeGuids = new HashSet<string>();

            for (int i = 0; i < elements.Count(); i++)
            {
                var element = elements[i];
                if (element is GraphNode graphNode)
                {
                    nodeGraphViewDataGuids.Add(graphNode.guid);
                    selectedNodeGuids.Add(graphNode.guid);
                }
                else if (element is IGraphEdge graphEdge)
                {
                    edgeDataGuids.Add(graphEdge.guid);
                }
            }
            
            var parentData = CurrentGraphViewNodeContext?.linkedData;
            if (parentData?.edgeDataList != null)
            {
                foreach (var edgeData in parentData.edgeDataList)
                {
                    var sw = edgeData.linkedElement;
                    if (sw == null) continue;

                    if (selectedNodeGuids.Contains(sw.inputElementGuid) &&
                        selectedNodeGuids.Contains(sw.outputElementGuid))
                    {
                        edgeDataGuids.Add(edgeData.guid);
                    }
                }
            }

            edgeDataGuids = edgeDataGuids.Distinct().ToList();

            elements.RemoveAll(e => e is not GraphNode);
            if (elements.Count == 0) return;

            var rootRectPos = elements[0].GetPosition();
            rootPos = new(rootRectPos.x, rootRectPos.y);
        }
    }
}