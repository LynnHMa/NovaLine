using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Utils.Ext;
using NovaLine.Data.Edge;
using NovaLine.Data.NodeGraphView;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Utils.Scope;
using NovaLine.Element;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static NovaLine.Editor.Window.WindowContextRegistry;

namespace NovaLine.Editor.Window
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
                    
                    var newGraphNode = parentGraphView.summonNewGraphNode(pastedNodeGraphViewData.linkedElement,actualPos);
                    parentGraphView.addGraphNode(newGraphNode);
                    
                    var newContext = parentGraphView.summonNewChildGraphContext(pastedNodeGraphViewData.linkedElement, actualPos);
                    newContext.linkedData = pastedNodeGraphViewData;
                    RegisterContext(newContext);
                }

                if (copiedNodeGraphViewDatas.Count == 0 || pastedNodeGraphViewDatas.Count == 0) return;
                
                for (var j = 0; j < copiedData.edgeDataGuids.Count; j++)
                {
                    var edgeDataGuid = copiedData.edgeDataGuids[j];
                    var copiedEdgeData = getChildEdgeData(parentData, edgeDataGuid);
                    var pastedEdgeData = (IEdgeData)copiedEdgeData.copy();
                    if (pastedEdgeData == null) return;

                    var inputElementIndex = copiedNodeGraphViewDatas.FindIndex(copiedNodeGraphViewData =>
                        copiedNodeGraphViewData.guid.Equals(copiedEdgeData.linkedSwitcher.inputElement.guid));
                    var outputElementIndex = copiedNodeGraphViewDatas.FindIndex(copiedNodeGraphViewData =>
                        copiedNodeGraphViewData.guid.Equals(copiedEdgeData.linkedSwitcher.outputElement.guid));
                    
                    if(inputElementIndex < 0 || inputElementIndex >= copiedNodeGraphViewDatas.Count || outputElementIndex < 0 || outputElementIndex >= copiedNodeGraphViewDatas.Count) continue;
                    
                    pastedEdgeData.linkedSwitcher.inputElement = pastedNodeGraphViewDatas[inputElementIndex]?.linkedElement;
                    pastedEdgeData.linkedSwitcher.outputElement = pastedNodeGraphViewDatas[outputElementIndex]?.linkedElement;
                    
                    var newGraphEdge = parentGraphView.summonNewGraphEdge(pastedEdgeData.linkedSwitcher);
                    parentGraphView.addGraphEdge(newGraphEdge);
                }
            }
        }
        
        public static bool OnCanPaste(string data)
        {
            return !String.IsNullOrEmpty(data);
        }

        private static IGraphViewNodeData getChildNodeGraphViewData(IGraphViewNodeData parentData,string guid)
        {
            return parentData.nodeDatas.Find(nodeData => nodeData.guid.Equals(guid));
        }

        private static IEdgeData getChildEdgeData(IGraphViewNodeData parentData, string guid)
        {
            return parentData.edgeDatas.Find(edgeData => edgeData.guid.Equals(guid));
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
            //Import linked data guid
            for (int i = 0; i < elements.Count(); i++)
            {
                var element = elements[i];
                if (element is GraphNode graphNode)
                {
                    nodeGraphViewDataGuids.Add(graphNode.guid);
                }
                else if (element is IGraphEdge graphEdge)
                {
                    edgeDataGuids.Add(graphEdge.guid);
                }
            }
            elements.RemoveAll(e => e is not GraphNode);
            var rootRectPos = elements[0].GetPosition();
            rootPos = new(rootRectPos.x, rootRectPos.y);
        }
    }
}