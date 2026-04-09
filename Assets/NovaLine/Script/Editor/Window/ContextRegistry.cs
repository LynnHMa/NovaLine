using System.Collections.Generic;
using System.Linq;
using NovaLine.Script.Data;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Window.Context;
using NovaLine.Script.Editor.Window.Context.Edge;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Editor.Window
{
    public static class ContextRegistry
    {
        public static Dictionary<string,INovaContext> RegisteredContexts { get; } = new();
        public static FlowchartNodeContext RegisteredFlowchartNodeContext => RegisteredContexts.FirstOrDefault(context => context.Value.type.Equals(NovaElementType.FLOWCHART)).Value as FlowchartNodeContext;
        
        public static EList<IGraphViewNodeContext> LoadedGraphViewContexts { get; } = new();
        public static IGraphViewNodeContext LastGraphViewNodeContext => LoadedGraphViewContexts.Count > 1 ? LoadedGraphViewContexts[1] : null;
        public static IGraphViewNodeContext CurrentGraphViewNodeContext => LoadedGraphViewContexts.Count > 0 ? LoadedGraphViewContexts[0] : null;
        
        public static void RegisterContext(INovaContext graphViewNodeContext)
        {
            if (graphViewNodeContext?.linkedData == null) return;

            if (graphViewNodeContext.linkedData is IGraphViewNodeData graphViewNodeData)
            {
                var nodeDataList = graphViewNodeData.nodeDataList;
                if (nodeDataList != null && nodeDataList.Count > 0)
                {
                    foreach (var nodeData in graphViewNodeData.nodeDataList)
                    {
                        var childContext = createContextByType(nodeData, nodeData.linkedElement.type);
                        RegisterContext(childContext);
                    }
                }
                var edgeDataList = graphViewNodeData.edgeDataList;
                if (edgeDataList != null && edgeDataList.Count > 0)
                {
                    foreach (var edgeData in graphViewNodeData.edgeDataList)
                    {
                        var edgeContext = new EdgeContext(edgeData);
                        RegisterContext(edgeContext);
                    }
                }
            
                if (graphViewNodeData is IHasConditionData hasConditionData)
                {
                    var conditionBeforeActionInvoke = hasConditionData.conditionBeforeInvokeData;
                    var conditionAfterActionInvoke = hasConditionData.conditionAfterInvokeData;
                
                    if (conditionBeforeActionInvoke != null)
                    {
                        var conditionBeforeInvokeContext = new ConditionContext(conditionBeforeActionInvoke);
                        RegisterContext(conditionBeforeInvokeContext);
                    }
                    if (conditionAfterActionInvoke != null)
                    {
                        var conditionAfterInvokeContext = new ConditionContext(conditionAfterActionInvoke);
                        RegisterContext(conditionAfterInvokeContext);
                    }
                }
            }
            else if (graphViewNodeContext.linkedData is IEdgeData edgeData)
            {
                if (edgeData is NodeEdgeData nodeEdgeData && nodeEdgeData.switchConditionData != null)
                {
                    var conditionContext = new ConditionContext(nodeEdgeData.switchConditionData);
                    RegisterContext(conditionContext);
                }
            }
            
            RegisteredContexts.TryAdd(graphViewNodeContext.guid,graphViewNodeContext);
        }
        public static void UnregisterContext(string contextGuid, NovaElementType type)
        {
            UnregisterContext(GetContext(contextGuid, type));
        }
        public static void UnregisterContext(INovaContext graphViewNodeContext)
        {
            if (graphViewNodeContext?.linkedData == null) return;

            if (graphViewNodeContext.linkedData is IGraphViewNodeData graphViewNodeData)
            {
                if (graphViewNodeData.nodeDataList != null && graphViewNodeData.nodeDataList.Count > 0)
                {
                    foreach (var nodeData in graphViewNodeData.nodeDataList)
                    {
                        if (nodeData?.linkedElement == null) continue;
                        var childContext = GetContext(nodeData.guid, nodeData.linkedElement.type);
                        UnregisterContext(childContext);
                    }
                }
                if (graphViewNodeData.edgeDataList != null && graphViewNodeData.edgeDataList.Count > 0)
                {
                    foreach (var edgeData in graphViewNodeData.edgeDataList)
                    {
                        UnregisterContext(edgeData.guid, NovaElementType.SWITCHER);
                    }
                }
                if (graphViewNodeData is IHasConditionData hasConditionData)
                {
                    UnregisterContext(hasConditionData.conditionBeforeInvokeData?.guid, NovaElementType.CONDITION);
                    UnregisterContext(hasConditionData.conditionAfterInvokeData?.guid, NovaElementType.CONDITION);
                }
            }
            else if (graphViewNodeContext.linkedData is IEdgeData edgeData)
            {
                if (edgeData is NodeEdgeData nodeEdgeData && nodeEdgeData.switchConditionData != null)
                {
                    UnregisterContext(nodeEdgeData.switchConditionData.guid,NovaElementType.CONDITION);
                }
            }

            RegisteredContexts.Remove(graphViewNodeContext.guid);
        }
        public static void ClearContexts()
        {
            RegisteredContexts.Clear();
            LoadedGraphViewContexts.Clear();
        }

        public static void ReplaceLinkedElement(NovaElement newElement)
        {
            var linkedContext = GetContext(newElement.guid, newElement.type);
            linkedContext.linkedData.updateLinkedElement(false);
        }
        
        public static INovaContext GetContext(GraphNode graphNode, NovaElementType type)
        {
            if (graphNode == null) return null;
            var guid = graphNode.guid ?? graphNode.linkedElement?.guid;
            return GetContext(guid, type);
        }
        public static INovaContext GetContext(string guid, NovaElementType type)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            return RegisteredContexts.TryGetValue(guid, out var ctx) && ctx.type == type ? ctx : null;
        }
        private static INovaContext createContextByType(INovaData linkedData,NovaElementType type)
        {
            switch (type)
            {
                case NovaElementType.NODE:
                    return new NodeNodeContext(linkedData as NodeData);
                case NovaElementType.ACTION:
                    return new ActionNodeContext(linkedData as ActionData);
                case NovaElementType.CONDITION:
                    return new ConditionContext(linkedData as ConditionData);
                case NovaElementType.EVENT:
                    return new EventNodeContext(linkedData as EventData);
                case NovaElementType.SWITCHER:
                    return new EdgeContext(linkedData as IEdgeData);
            }

            return null;
        }
    }
}
