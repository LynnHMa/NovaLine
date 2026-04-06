using System.Collections.Generic;
using System.Linq;
using Editor.Utils.Ext;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Window.Context;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Editor.Window
{
    public static class ContextRegistry
    {
        public static Dictionary<string,IGraphViewContext> RegisteredContexts { get; } = new();
        public static FlowchartContext RegisteredFlowchartContext => RegisteredContexts.FirstOrDefault(context => context.Value.type.Equals(NovaElementType.FLOWCHART)).Value as FlowchartContext;
        
        public static EList<IGraphViewContext> LoadedGraphViewContexts { get; } = new();
        public static IGraphViewContext LastGraphViewContext => LoadedGraphViewContexts.Count > 1 ? LoadedGraphViewContexts[1] : null;
        public static IGraphViewContext CurrentGraphViewContext => LoadedGraphViewContexts.Count > 0 ? LoadedGraphViewContexts[0] : null;
        
        public static void RegisterContext(IGraphViewContext graphViewContext)
        {
            if (graphViewContext == null) return;
            
            foreach (var nodeData in graphViewContext.linkedData.nodeDataList)
            {
                var childContext = createContextByType(nodeData, nodeData.linkedElement.type);
                RegisterContext(childContext);
            }
            
            if (graphViewContext.linkedData is IHasConditionData hasConditionData)
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
            RegisteredContexts.TryAdd(graphViewContext.guid,graphViewContext);
        }
        public static void UnregisterContext(string contextGuid, NovaElementType type)
        {
            UnregisterContext(GetContext(contextGuid, type));
        }
        public static void UnregisterContext(IGraphViewContext graphViewContext)
        {
            if (graphViewContext == null) return;
            if (graphViewContext.linkedData == null) return;

            if (graphViewContext.linkedData.nodeDataList != null)
            {
                foreach (var nodeData in graphViewContext.linkedData.nodeDataList)
                {
                    if (nodeData == null || nodeData.linkedElement == null) continue;
                    var childContext = GetContext(nodeData.guid, nodeData.linkedElement.type);
                    UnregisterContext(childContext);
                }
            }
            
            // Unregister edge contexts (for NodeEdgeData's switchCondition)
            if (graphViewContext.linkedData.edgeDataList != null)
            {
                foreach (var edgeData in graphViewContext.linkedData.edgeDataList)
                {
                    if (edgeData is NovaLine.Script.Data.Edge.NodeEdgeData nodeEdgeData)
                    {
                        if (nodeEdgeData.switchConditionData != null)
                        {
                            UnregisterContext(nodeEdgeData.switchConditionData.guid, NovaElementType.CONDITION);
                        }
                    }
                }
            }

            if (graphViewContext.linkedData is IHasConditionData hasConditionData)
            {
                UnregisterContext(hasConditionData.conditionBeforeInvokeData?.guid, NovaElementType.CONDITION);
                UnregisterContext(hasConditionData.conditionAfterInvokeData?.guid, NovaElementType.CONDITION);
            }

            RegisteredContexts.Remove(graphViewContext.guid);
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
        
        public static IGraphViewContext GetContext(GraphNode graphNode, NovaElementType type)
        {
            if (graphNode == null) return null;
            var guid = graphNode.guid ?? graphNode.linkedElement?.guid;
            return GetContext(guid, type);
        }
        public static IGraphViewContext GetContext(string guid, NovaElementType type)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            return RegisteredContexts.TryGetValue(guid, out var ctx) && ctx.type == type ? ctx : null;
        }
        private static IGraphViewContext createContextByType(IGraphViewNodeData linkedData,NovaElementType type)
        {
            switch (type)
            {
                case NovaElementType.NODE:
                    return new NodeContext(linkedData as NodeData);
                case NovaElementType.ACTION:
                    return new ActionContext(linkedData as ActionData);
                case NovaElementType.CONDITION:
                    return new ConditionContext(linkedData as ConditionData);
                case NovaElementType.EVENT:
                    return new EventContext(linkedData as EventData);
            }

            return null;
        }
    }
}
