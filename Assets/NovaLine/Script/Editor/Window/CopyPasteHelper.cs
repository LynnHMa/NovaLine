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
                var parentContext = GetContext(copiedData.linkedContextInfo.key,copiedData.linkedContextInfo.value);
                var parentData = parentContext.linkedData;
                var parentGraphView = parentContext.graphView;
            
                if (parentData == null || parentGraphView == null) return;

                var copiedNodeGraphViewDatas = new List<IGraphViewNodeData>();
                var pastedNodeGraphViewDatas = new List<IGraphViewNodeData>();
                for (var i = 0; i < copiedData.nodeGraphViewDataGuids.Count; i++)
                {
                    var nodeGraphViewDataGuid = copiedData.nodeGraphViewDataGuids[i];
                    
                    var copiedNodeGraphViewData = getChildNodeGraphViewData(parentData, nodeGraphViewDataGuid);
                    var pastedNodeGraphViewData = (IGraphViewNodeData)copiedNodeGraphViewData?.copy();
                    if(copiedNodeGraphViewData == null || pastedNodeGraphViewData == null) continue;
                    
                    copiedNodeGraphViewDatas.Add(copiedNodeGraphViewData);
                    pastedNodeGraphViewDatas.Add(pastedNodeGraphViewData);
                    
                    var actualPos = parentGraphView.mousePos + pastedNodeGraphViewData.pos - copiedData.rootPos;

                    pastedNodeGraphViewData.pos = actualPos;
                    var newContext = parentGraphView.summonNewChildGraphContext(pastedNodeGraphViewData);
                    newContext.linkedData = pastedNodeGraphViewData;
                    newContext.linkedData.linkedElement.parentGuid = parentGraphView.linkedElementGuid;
                    RegisterContext(newContext);
                    
                    CommandRegistry.Register(new AddNodeCommand(
                        parentGraphView.linkedElementGuid,
                        parentGraphView.linkedElement.type,
                        pastedNodeGraphViewData.strongCopy() as IGraphViewNodeData));
                    
                    pastedNodeGraphViewData.linkedElement.setParent(parentGraphView.linkedElement as NovaElement);
                    var newGraphNode = parentGraphView.summonNewGraphNode(pastedNodeGraphViewData.linkedElement, actualPos);
                    parentGraphView.addGraphNode(newGraphNode);
                }

                if (copiedNodeGraphViewDatas.Count == 0 || pastedNodeGraphViewDatas.Count == 0) return;
                
                for (var j = 0; j < copiedData.edgeDataGuids.Count; j++)
                {
                    var edgeDataGuid = copiedData.edgeDataGuids[j];
                    var copiedEdgeData = getChildEdgeData(parentData, edgeDataGuid);
                    if (copiedEdgeData == null) continue;
                    var pastedEdgeData = (IEdgeData)copiedEdgeData.copy();
                    if (pastedEdgeData == null) continue;

                    var inputElementIndex = copiedNodeGraphViewDatas.FindIndex(copiedNodeGraphViewData =>
                        copiedNodeGraphViewData.guid.Equals(copiedEdgeData.linkedSwitcher.inputElementGuid));
                    var outputElementIndex = copiedNodeGraphViewDatas.FindIndex(copiedNodeGraphViewData =>
                        copiedNodeGraphViewData.guid.Equals(copiedEdgeData.linkedSwitcher.outputElementGuid));
                    
                    if(inputElementIndex < 0 || inputElementIndex >= copiedNodeGraphViewDatas.Count || outputElementIndex < 0 || outputElementIndex >= copiedNodeGraphViewDatas.Count) continue;
                    
                    pastedEdgeData.linkedSwitcher.inputElementGuid = pastedNodeGraphViewDatas[inputElementIndex]?.linkedElement?.guid;
                    pastedEdgeData.linkedSwitcher.outputElementGuid = pastedNodeGraphViewDatas[outputElementIndex]?.linkedElement?.guid;
                    
                    var newGraphEdge = parentGraphView.summonNewGraphEdge(pastedEdgeData.linkedSwitcher);
                    parentGraphView.addGraphEdgeByHand(newGraphEdge);
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
            linkedContextInfo = new(CurrentGraphViewContext.guid, CurrentGraphViewContext.type);
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

            // 自动补全：把选中节点之间的内部边也带上
            var parentData = CurrentGraphViewContext?.linkedData;
            if (parentData?.edgeDataList != null)
            {
                foreach (var edgeData in parentData.edgeDataList)
                {
                    var sw = edgeData.linkedSwitcher;
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