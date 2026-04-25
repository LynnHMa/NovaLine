using System;
using System.Collections.Generic;
using System.Linq;
using NovaLine.Script.Data;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Graph.View;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window;
using NovaLine.Script.Editor.Window.Command;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Interface;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static NovaLine.Script.Editor.Window.ContextRegistry;

namespace NovaLine.Script.Editor.Utils.Ext
{
    public static class EditorDataExt
    {
        private static INovaGraphView CurrentGraphView => CurrentGraphViewNodeContext.GraphView;
        public static bool HasGraphView(this INovaData data)
        {
            return data is FlowchartData or NodeData or ConditionData;
        }

        public static bool HasGraphNode(this INovaData data)
        {
            return data is NodeData or ActionData or EventData;
        }
        
        public static void InstantiateDataIntoCurrentGraphView(InstantiatableData beforeRebind)
        {
            using (new CommandScope())
            using (new SaveScope())
            {
                var afterRebind = Rebind(beforeRebind);
                
                if (afterRebind == null || CurrentGraphView == null) return;
                
                var rebindingNodeGraphViewDataList = afterRebind.nodeGraphViewDataList;
                for (var i = 0; i < rebindingNodeGraphViewDataList.Count; i++)
                {
                    var rebindingNodeGraphViewData = rebindingNodeGraphViewDataList[i];
                    var newContext = CurrentGraphView.SummonNewChildGraphViewNodeContext(rebindingNodeGraphViewData);
                    RegisterContext(newContext);
                    
                    CommandRegistry.RegisterCommand(new AddNodeCommand(
                        CurrentGraphView.LinkedElementGUID,
                        CurrentGraphView.LinkedElement.Type,
                        rebindingNodeGraphViewData.StrongCopy() as IGraphViewNodeData));
                    
                    var newGraphNode = CurrentGraphView.SummonNewGraphNode(rebindingNodeGraphViewData.LinkedElement, rebindingNodeGraphViewData.Pos);
                    CurrentGraphView.AddGraphNode(newGraphNode);
                    if(i == rebindingNodeGraphViewDataList.Count - 1) rebindingNodeGraphViewData.LinkedElement.ShowInInspector();
                }

                var rebindingEdgeDataList = afterRebind.edgeDataList;
                for (var j = 0; j < rebindingEdgeDataList.Count; j++)
                {
                    var rebindingEdgeData = rebindingEdgeDataList[j];
                    
                    var newEdgeContext = CurrentGraphView.SummonNewChildEdgeContext(rebindingEdgeData);
                    RegisterContext(newEdgeContext);
                    
                    CommandRegistry.RegisterCommand(new AddEdgeCommand(
                        CurrentGraphView.LinkedElementGUID,
                        CurrentGraphView.LinkedElement.Type,
                        rebindingEdgeData.StrongCopy() as IEdgeData));
                    
                    var newGraphEdge = CurrentGraphView.SummonNewGraphEdge(rebindingEdgeData.LinkedElement);
                    CurrentGraphView.AddGraphEdge(newGraphEdge);
                }
            }
        }
        public static void InstantiateDataToReplaceNodeGraphView(InstantiatableData beforeRebind, NovaElement beforeElement)
        {
            using (new SaveScope())
            {
                if (beforeRebind.nodeGraphViewDataList.FirstOrDefault()?.Copy() is not IGraphViewNodeData afterData) return;

                var beforeContext = GetContext(beforeElement.GUID, beforeElement.Type);
                var beforeData = beforeContext?.LinkedData;
                if (beforeData == null) return;
                
                var replacingGraphView = CurrentGraphView.LinkedElement.Type.Equals(beforeElement.Type);
                
                InspectorHelper.SetSuppressValueChange(true);
                try
                {
                    if (replacingGraphView) NovaWindow.ExitGraphView();
                }
                finally
                {
                    InspectorHelper.SetSuppressValueChange(false);
                }
                
                InspectorHelper.GlobalReplace(beforeData,afterData);
                
                if (replacingGraphView)
                {
                    if (GetContext(afterData.GUID, afterData.Type) is IGraphViewNodeContext afterContext)
                    {
                        NovaWindow.LoadContextInWindow(afterContext);
                    }
                }
                
                afterData.LinkedElement.ShowInInspector();
            }
        }
        
        private static InstantiatableData Rebind(InstantiatableData beforeRebind)
        {
            if (GetContext(beforeRebind.linkedContextInfo.key,beforeRebind.linkedContextInfo.value) is not IGraphViewNodeContext parentContext) return null;
            if (CurrentGraphViewNodeContext == null || CurrentGraphViewNodeContext.Type != parentContext.Type) return null;
            
            var currentGraphView = CurrentGraphViewNodeContext.GraphView;
            if (currentGraphView == null) return null;
            
            var oriNodeGraphViewNodeDataList = beforeRebind.nodeGraphViewDataList;
            var rebindingNodeGraphViewDataList = new List<IGraphViewNodeData>();
            for (var i = 0; i < oriNodeGraphViewNodeDataList.Count; i++)
            {
                var oriNodeGraphViewData = oriNodeGraphViewNodeDataList[i];
                var rebindingNodeGraphViewData = (IGraphViewNodeData)oriNodeGraphViewData?.Copy();
                    
                if(oriNodeGraphViewData == null || rebindingNodeGraphViewData == null) continue;
                    
                var actualPos = currentGraphView.MousePos + rebindingNodeGraphViewData.Pos - beforeRebind.rootPos;

                rebindingNodeGraphViewData.Pos = actualPos;
                RebindGraphViewNodeDataParent(rebindingNodeGraphViewData, currentGraphView.LinkedElement as NovaElement);
                    
                rebindingNodeGraphViewDataList.Add(rebindingNodeGraphViewData);
            }

            var oriEdgeDataList = beforeRebind.edgeDataList;
            var rebindingEdgeDataList = new List<IEdgeData>();
            for (var j = 0; j < oriEdgeDataList.Count; j++)
            {
                var oriEdgeData = oriEdgeDataList[j];
                var rebindingEdgeData = (IEdgeData)oriEdgeData?.Copy();
                    
                if (oriEdgeData == null || rebindingEdgeData == null) continue;

                var inputElementIndex = oriNodeGraphViewNodeDataList.FindIndex(oriNodeGraphViewData =>
                    oriNodeGraphViewData.GUID.Equals(oriEdgeData.LinkedElement.InputElementGUID));
                var outputElementIndex = oriNodeGraphViewNodeDataList.FindIndex(oriNodeGraphViewData =>
                    oriNodeGraphViewData.GUID.Equals(oriEdgeData.LinkedElement.OutputElementGUID));
                    
                if(inputElementIndex < 0 || inputElementIndex >= oriNodeGraphViewNodeDataList.Count || outputElementIndex < 0 || outputElementIndex >= oriNodeGraphViewNodeDataList.Count) continue;
                    
                rebindingEdgeData.LinkedElement.InputElementGUID = rebindingNodeGraphViewDataList[inputElementIndex]?.LinkedElement?.GUID;
                rebindingEdgeData.LinkedElement.OutputElementGUID = rebindingNodeGraphViewDataList[outputElementIndex]?.LinkedElement?.GUID;

                RebindEdgeDataParent(rebindingEdgeData, currentGraphView.LinkedElement as NovaElement);
                    
                rebindingEdgeDataList.Add(rebindingEdgeData);
            }

            return new InstantiatableData(beforeRebind.linkedContextInfo, rebindingNodeGraphViewDataList,
                rebindingEdgeDataList, beforeRebind.rootPos);
        }
        private static void RebindGraphViewNodeDataParent(IGraphViewNodeData graphViewNodeData, NovaElement parentElement)
        {
            if (graphViewNodeData?.LinkedElement == null) return;

            graphViewNodeData.LinkedElement.SetParent(parentElement);

            if (graphViewNodeData is IAroundConditionData hasConditionData)
            {
                RebindGraphViewNodeDataParent(hasConditionData.ConditionBeforeInvokeData, graphViewNodeData.LinkedElement);
                RebindGraphViewNodeDataParent(hasConditionData.ConditionAfterInvokeData, graphViewNodeData.LinkedElement);
            }

            if (graphViewNodeData.NodeDataList != null)
            {
                foreach (var childNodeData in graphViewNodeData.NodeDataList)
                {
                    RebindGraphViewNodeDataParent(childNodeData, graphViewNodeData.LinkedElement);
                }
            }

            if (graphViewNodeData.EdgeDataList != null)
            {
                foreach (var childEdgeData in graphViewNodeData.EdgeDataList)
                {
                    RebindEdgeDataParent(childEdgeData, graphViewNodeData.LinkedElement);
                }
            }
        }
        private static void RebindEdgeDataParent(IEdgeData edgeData, NovaElement parentElement)
        {
            if (edgeData?.LinkedElement == null) return;

            edgeData.LinkedElement.SetParent(parentElement);

            if (edgeData is NodeEdgeData nodeEdgeData)
            {
                RebindGraphViewNodeDataParent(nodeEdgeData.SwitchConditionData, edgeData.LinkedElement);
            }
        }
    }
    
    [Serializable]
    public class InstantiatableData
    {
        public KeyValue<string,NovaElementType> linkedContextInfo;
        
        [SerializeReference]
        public List<IGraphViewNodeData> nodeGraphViewDataList = new();
        
        [SerializeReference]
        public List<IEdgeData> edgeDataList = new();
        
        public Vector2 rootPos = Vector2.zero;
        
        public InstantiatableData(IEnumerable<GraphElement> selectedElements)
        {
            linkedContextInfo = new(CurrentGraphViewNodeContext.GUID, CurrentGraphViewNodeContext.Type);
            if (selectedElements == null) return;

            var elements = selectedElements.OrderBy(e => e.GetPosition().y).ThenBy(e => e.GetPosition().x).ToList();
            if (elements.Count == 0) return;
            
            var selectedNodeGUIDs = new HashSet<string>();
            for (int i = 0; i < elements.Count(); i++)
            {
                var element = elements[i];
                switch (element)
                {
                    case GraphNode graphNode:
                        nodeGraphViewDataList.Add(FindGraphNodeLinkedData(graphNode.GUID,graphNode.Type) as IGraphViewNodeData);
                        selectedNodeGUIDs.Add(graphNode.GUID);
                        break;
                    case IGraphEdge graphEdge:
                        edgeDataList.Add(FindGraphNodeLinkedData(graphEdge.GUID,NovaElementType.Switcher) as IEdgeData);
                        break;
                }
            }
            
            var parentData = CurrentGraphViewNodeContext?.LinkedData;
            if (parentData?.EdgeDataList != null)
            {
                foreach (var edgeData in parentData.EdgeDataList)
                {
                    var sw = edgeData.LinkedElement;
                    if (sw == null) continue;

                    if (selectedNodeGUIDs.Contains(sw.InputElementGUID) && selectedNodeGUIDs.Contains(sw.OutputElementGUID))
                    {
                        edgeDataList.Add(edgeData);
                    }
                }
            }

            edgeDataList = edgeDataList.Distinct().ToList();

            elements.RemoveAll(e => e is not GraphNode);
            if (elements.Count == 0) return;

            var rootRectPos = elements[0].GetPosition();
            rootPos = new(rootRectPos.x, rootRectPos.y);
        }
        public InstantiatableData(IGraphViewNodeData linkedNodeData,Vector2 rootPos)
        {
            linkedContextInfo = new(CurrentGraphViewNodeContext.GUID, CurrentGraphViewNodeContext.Type);
            nodeGraphViewDataList.Add(linkedNodeData);
            this.rootPos = rootPos;
        }

        public InstantiatableData(KeyValue<string, NovaElementType> linkedContextInfo,
            List<IGraphViewNodeData> nodeGraphViewDataList, List<IEdgeData> edgeDataList, Vector2 rootPos)
        {
            this.linkedContextInfo = linkedContextInfo;
            this.nodeGraphViewDataList = nodeGraphViewDataList;
            this.edgeDataList = edgeDataList;
            this.rootPos = rootPos;
        }

        private INovaData FindGraphNodeLinkedData(string graphNodeGUID,NovaElementType type)
        {
            return GetContext(graphNodeGUID,type)?.LinkedData;
        }
    }
}
