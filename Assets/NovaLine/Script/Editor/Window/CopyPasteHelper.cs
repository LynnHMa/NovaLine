﻿using System;
using System.Collections.Generic;
using System.Linq;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Utils.Ext;
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
                if (CurrentGraphViewNodeContext == null || CurrentGraphViewNodeContext.Type != parentContext.Type) return;
                var parentData = parentContext.LinkedData;
                
                var currentGraphView = CurrentGraphViewNodeContext.GraphView;
            
                if (parentData == null || currentGraphView == null) return;

                var copiedNodeGraphViewDataList = new List<IGraphViewNodeData>();
                var pastedNodeGraphViewDataList = new List<IGraphViewNodeData>();
                for (var i = 0; i < copiedData.nodeGraphViewDataGuids.Count; i++)
                {
                    var nodeGraphViewDataGuid = copiedData.nodeGraphViewDataGuids[i];
                    
                    var copiedNodeGraphViewData = GetChildNodeGraphViewData(parentData, nodeGraphViewDataGuid);
                    var pastedNodeGraphViewData = (IGraphViewNodeData)copiedNodeGraphViewData?.copy();
                    if(copiedNodeGraphViewData == null || pastedNodeGraphViewData == null) continue;
                    
                    copiedNodeGraphViewDataList.Add(copiedNodeGraphViewData);
                    pastedNodeGraphViewDataList.Add(pastedNodeGraphViewData);
                    
                    var actualPos = currentGraphView.MousePos + pastedNodeGraphViewData.pos - copiedData.rootPos;

                    pastedNodeGraphViewData.pos = actualPos;
                    var newContext = currentGraphView.SummonNewChildGraphViewNodeContext(pastedNodeGraphViewData);
                    newContext.LinkedData = pastedNodeGraphViewData;
                    RegisterContext(newContext);
                    
                    CommandRegistry.RegisterCommand(new AddNodeCommand(
                        currentGraphView.LinkedElementGuid,
                        currentGraphView.LinkedElement.Type,
                        pastedNodeGraphViewData.strongCopy() as IGraphViewNodeData));
                    
                    pastedNodeGraphViewData.linkedElement.SetParent(currentGraphView.LinkedElement as NovaElement);
                    var newGraphNode = currentGraphView.SummonNewGraphNode(pastedNodeGraphViewData.linkedElement, actualPos);
                    currentGraphView.AddGraphNode(newGraphNode);
                }

                if (copiedNodeGraphViewDataList.Count == 0 || pastedNodeGraphViewDataList.Count == 0) return;
                
                for (var j = 0; j < copiedData.edgeDataGuids.Count; j++)
                {
                    var edgeDataGuid = copiedData.edgeDataGuids[j];
                    var copiedEdgeData = GetChildEdgeData(parentData, edgeDataGuid);
                    if (copiedEdgeData == null) continue;
                    var pastedEdgeData = (IEdgeData)copiedEdgeData.copy();
                    if (pastedEdgeData == null) continue;

                    var inputElementIndex = copiedNodeGraphViewDataList.FindIndex(copiedNodeGraphViewData =>
                        copiedNodeGraphViewData.Guid.Equals(copiedEdgeData.linkedElement.inputElementGuid));
                    var outputElementIndex = copiedNodeGraphViewDataList.FindIndex(copiedNodeGraphViewData =>
                        copiedNodeGraphViewData.Guid.Equals(copiedEdgeData.linkedElement.outputElementGuid));
                    
                    if(inputElementIndex < 0 || inputElementIndex >= copiedNodeGraphViewDataList.Count || outputElementIndex < 0 || outputElementIndex >= copiedNodeGraphViewDataList.Count) continue;
                    
                    pastedEdgeData.linkedElement.inputElementGuid = pastedNodeGraphViewDataList[inputElementIndex]?.linkedElement?.Guid;
                    pastedEdgeData.linkedElement.outputElementGuid = pastedNodeGraphViewDataList[outputElementIndex]?.linkedElement?.Guid;

                    var newEdgeContext = currentGraphView.SummonNewChildEdgeContext(pastedEdgeData);
                    RegisterContext(newEdgeContext);
                    pastedEdgeData.linkedElement.SetParent(currentGraphView.LinkedElement as NovaElement);
                    
                    CommandRegistry.RegisterCommand(new AddEdgeCommand(
                        currentGraphView.LinkedElementGuid,
                        currentGraphView.LinkedElement.Type,
                        pastedEdgeData.strongCopy() as IEdgeData));
                    
                    var newGraphEdge = currentGraphView.SummonNewGraphEdge(pastedEdgeData.linkedElement);
                    currentGraphView.AddGraphEdge(newGraphEdge);
                }
            }
        }
        
        public static bool OnCanPaste(string data)
        {
            return !String.IsNullOrEmpty(data);
        }

        private static IGraphViewNodeData GetChildNodeGraphViewData(IGraphViewNodeData parentData,string guid)
        {
            return parentData.nodeDataList.Find(nodeData => nodeData.Guid.Equals(guid));
        }

        private static IEdgeData GetChildEdgeData(IGraphViewNodeData parentData, string guid)
        {
            return parentData.edgeDataList.Find(edgeData => edgeData.Guid.Equals(guid));
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
            linkedContextInfo = new(CurrentGraphViewNodeContext.Guid, CurrentGraphViewNodeContext.Type);
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
                    edgeDataGuids.Add(graphEdge.Guid);
                }
            }
            
            var parentData = CurrentGraphViewNodeContext?.LinkedData;
            if (parentData?.edgeDataList != null)
            {
                foreach (var edgeData in parentData.edgeDataList)
                {
                    var sw = edgeData.linkedElement;
                    if (sw == null) continue;

                    if (selectedNodeGuids.Contains(sw.inputElementGuid) &&
                        selectedNodeGuids.Contains(sw.outputElementGuid))
                    {
                        edgeDataGuids.Add(edgeData.Guid);
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