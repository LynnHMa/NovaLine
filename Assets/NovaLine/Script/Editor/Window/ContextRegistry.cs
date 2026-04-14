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
        public static FlowchartNodeContext RegisteredFlowchartNodeContext => RegisteredContexts.FirstOrDefault(context => context.Value.Type.Equals(NovaElementType.FLOWCHART)).Value as FlowchartNodeContext;
        
        public static EList<IGraphViewNodeContext> LoadedGraphViewContexts { get; } = new();
        public static IGraphViewNodeContext LastGraphViewNodeContext => LoadedGraphViewContexts.Count > 1 ? LoadedGraphViewContexts[1] : null;
        public static IGraphViewNodeContext CurrentGraphViewNodeContext => LoadedGraphViewContexts.Count > 0 ? LoadedGraphViewContexts[0] : null;
        
        public static void RegisterContext(INovaContext graphViewNodeContext)
        {
            if (graphViewNodeContext?.LinkedData == null) return;

            switch (graphViewNodeContext.LinkedData)
            {
                case IGraphViewNodeData graphViewNodeData:
                {
                    var nodeDataList = graphViewNodeData.nodeDataList;
                    if (nodeDataList != null && nodeDataList.Count > 0)
                    {
                        foreach (var nodeData in graphViewNodeData.nodeDataList)
                        {
                            var childContext = CreateContextByType(nodeData, nodeData.linkedElement.Type);
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
                        var conditionBeforeActionInvoke = hasConditionData.ConditionBeforeInvokeData;
                        var conditionAfterActionInvoke = hasConditionData.ConditionAfterInvokeData;
                
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

                    break;
                }
                case IEdgeData edgeData:
                {
                    if (edgeData is NodeEdgeData nodeEdgeData && nodeEdgeData.switchConditionData != null)
                    {
                        var conditionContext = new ConditionContext(nodeEdgeData.switchConditionData);
                        RegisterContext(conditionContext);
                    }

                    break;
                }
            }
            
            RegisteredContexts.TryAdd(graphViewNodeContext.Guid,graphViewNodeContext);
        }
        public static void UnregisterContext(string contextGuid, NovaElementType type)
        {
            UnregisterContext(GetContext(contextGuid, type));
        }
        public static void UnregisterContext(INovaContext graphViewNodeContext)
        {
            if (graphViewNodeContext?.LinkedData == null) return;

            if (graphViewNodeContext.LinkedData is IGraphViewNodeData graphViewNodeData)
            {
                if (graphViewNodeData.nodeDataList != null && graphViewNodeData.nodeDataList.Count > 0)
                {
                    foreach (var nodeData in graphViewNodeData.nodeDataList)
                    {
                        if (nodeData?.linkedElement == null) continue;
                        var childContext = GetContext(nodeData.Guid, nodeData.linkedElement.Type);
                        UnregisterContext(childContext);
                    }
                }
                if (graphViewNodeData.edgeDataList != null && graphViewNodeData.edgeDataList.Count > 0)
                {
                    foreach (var edgeData in graphViewNodeData.edgeDataList)
                    {
                        UnregisterContext(edgeData.Guid, NovaElementType.SWITCHER);
                    }
                }
                if (graphViewNodeData is IHasConditionData hasConditionData)
                {
                    UnregisterContext(hasConditionData.ConditionBeforeInvokeData?.Guid, NovaElementType.CONDITION);
                    UnregisterContext(hasConditionData.ConditionAfterInvokeData?.Guid, NovaElementType.CONDITION);
                }
            }
            else if (graphViewNodeContext.LinkedData is IEdgeData edgeData)
            {
                if (edgeData is NodeEdgeData nodeEdgeData && nodeEdgeData.switchConditionData != null)
                {
                    UnregisterContext(nodeEdgeData.switchConditionData.Guid,NovaElementType.CONDITION);
                }
            }

            RegisteredContexts.Remove(graphViewNodeContext.Guid);
        }
        public static void ClearContexts()
        {
            RegisteredContexts.Clear();
            LoadedGraphViewContexts.Clear();
        }

        public static void ReplaceLinkedElementInContext(NovaElement newElement)
        {
            var linkedContext = GetContext(newElement.Guid, newElement.Type);
            linkedContext.LinkedData.updateLinkedElement(false);
        }
        
        public static INovaContext GetContext(GraphNode graphNode, NovaElementType type)
        {
            if (graphNode == null) return null;
            var guid = graphNode.guid ?? graphNode.linkedElement?.Guid;
            return GetContext(guid, type);
        }
        public static INovaContext GetContext(string guid, NovaElementType type)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            return RegisteredContexts.TryGetValue(guid, out var ctx) && ctx.Type == type ? ctx : null;
        }
        private static INovaContext CreateContextByType(INovaData linkedData,NovaElementType type)
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
