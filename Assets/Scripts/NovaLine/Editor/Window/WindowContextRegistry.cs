using System;
using System.Collections.Generic;
using Editor.Utils.Ext;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Utils.Ext;
using NovaLine.Editor.Window.Context;
using NovaLine.Element;
using UnityEngine;

namespace NovaLine.Editor.Window
{
    public static class WindowContextRegistry
    {
        public static FlowchartContext RegisteredFlowchartContext { get; set; }
        public static EList<NodeContext> RegisteredNodeContexts { get; } = new();
        public static EList<ActionContext> RegisteredActionContexts { get; } = new();
        public static EList<ConditionContext> RegisteredConditionContexts { get; } = new();
        public static EList<EventContext> RegisteredEventContexts { get; } = new();

        public static EList<IGraphViewContext> LoadedGraphViewContexts { get; } = new();
        public static IGraphViewContext LastGraphViewContext => LoadedGraphViewContexts.Count > 1 ? LoadedGraphViewContexts[1] : null;
        public static IGraphViewContext CurrentGraphViewContext => LoadedGraphViewContexts.Count > 0 ? LoadedGraphViewContexts[0] : null;

        public static void RegisterContext(IGraphViewContext graphViewContext)
        {
            if (graphViewContext is FlowchartContext flowchartContext)
            {
                RegisteredFlowchartContext = flowchartContext;
                foreach (var nodeData in flowchartContext.linkedData.nodeDatas)
                {
                    var nodeContext = new NodeContext(nodeData);
                    RegisterContext(nodeContext);
                }
                //When drawing, Condition context of node edges is registered.
            }
            else if (graphViewContext is NodeContext nodeContext)
            {
                foreach (var actionData in nodeContext.linkedData.nodeDatas)
                {
                    var actionContext = new ActionContext(actionData);
                    RegisterContext(actionContext);
                }

                var conditionBeforeNodeInvoke = nodeContext.linkedData?.conditionBeforeInvokeData;
                var conditionAfterNodeInvoke = nodeContext.linkedData?.conditionAfterInvokeData;
                if (conditionBeforeNodeInvoke != null)
                {
                    var conditionBeforeInvokeContext = new ConditionContext(conditionBeforeNodeInvoke);
                    RegisterContext(conditionBeforeInvokeContext);
                }
                if (conditionAfterNodeInvoke != null)
                {
                    var conditionAfterInvokeContext = new ConditionContext(conditionAfterNodeInvoke);
                    RegisterContext(conditionAfterInvokeContext);
                }

                RegisteredNodeContexts.Add(nodeContext);
            }
            else if (graphViewContext is ActionContext actionContext)
            {
                var conditionBeforeActionInvoke = actionContext.linkedData?.conditionBeforeInvokeData;
                var conditionAfterActionInvoke = actionContext.linkedData?.conditionAfterInvokeData;
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

                RegisteredActionContexts.Add(actionContext);
            }
            else if (graphViewContext is ConditionContext conditionContext)
            {
                foreach (var eventData in conditionContext.linkedData.nodeDatas)
                {
                    var eventContext = new EventContext(eventData);
                    RegisterContext(eventContext);
                }
                RegisteredConditionContexts.Add(conditionContext);
            }
            else if (graphViewContext is EventContext eventContext)
            {
                RegisteredEventContexts.Add(eventContext);
            }
        }
        public static void UnregisterContext(string contextGuid, NovaElementType type)
        {
            UnregisterContext(GetContext(contextGuid, type));
        }
        public static void UnregisterContext(IGraphViewContext graphViewContext)
        {
            if (graphViewContext is FlowchartContext flowchartContext)
            {
                foreach (var nodeData in flowchartContext.linkedData.nodeDatas)
                {
                    UnregisterContext(nodeData.guid, NovaElementType.NODE);
                }
                foreach (var nodeEdgeData in flowchartContext.linkedData.edgeDatas)
                {
                    UnregisterContext(nodeEdgeData.guid, NovaElementType.CONDITION);
                }
                RegisteredFlowchartContext = null;
            }
            else if (graphViewContext is NodeContext nodeContext)
            {
                foreach (var actionData in nodeContext.linkedData.nodeDatas)
                {
                    UnregisterContext(actionData.guid, NovaElementType.ACTION);
                }
                UnregisterContext(nodeContext.linkedData?.conditionBeforeInvokeData?.guid, NovaElementType.CONDITION);
                UnregisterContext(nodeContext.linkedData?.conditionAfterInvokeData?.guid, NovaElementType.CONDITION);
                RegisteredNodeContexts.Remove(nodeContext);
            }
            else if (graphViewContext is ActionContext actionContext)
            {
                UnregisterContext(actionContext.linkedData?.conditionBeforeInvokeData?.guid, NovaElementType.CONDITION);
                UnregisterContext(actionContext.linkedData?.conditionAfterInvokeData?.guid, NovaElementType.CONDITION);
                RegisteredActionContexts.Remove(actionContext);
            }
            else if (graphViewContext is ConditionContext conditionContext)
            {
                foreach (var eventData in conditionContext.linkedData.nodeDatas)
                {
                    UnregisterContext(eventData.guid, NovaElementType.EVENT);
                }
                RegisteredConditionContexts.Remove(conditionContext);
            }
            else if (graphViewContext is EventContext eventContext)
            {
                RegisteredEventContexts.Remove(eventContext);
            }
        }
        public static void ClearContexts()
        {
            RegisteredFlowchartContext = null;
            RegisteredNodeContexts.Clear();
            RegisteredActionContexts.Clear();
            RegisteredConditionContexts.Clear();
            RegisteredEventContexts.Clear();
            LoadedGraphViewContexts.Clear();
        }

        public static IGraphViewContext GetContext(GraphNode graphNode, NovaElementType type)
        {
            return GetContext(graphNode?.guid, type);
        }
        public static IGraphViewContext GetContext(string guid, NovaElementType type)
        {
            if (type == NovaElementType.NONE) return null;
            else if (RegisteredFlowchartContext.guid.Equals(guid) && type.Equals(NovaElementType.FLOWCHART)) return RegisteredFlowchartContext;
            else if (RegisteredNodeContexts.Get(guid) != null && type.Equals(NovaElementType.NODE)) return RegisteredNodeContexts.Get(guid);
            else if (RegisteredActionContexts.Get(guid) != null && type.Equals(NovaElementType.ACTION)) return RegisteredActionContexts.Get(guid);
            else if (RegisteredConditionContexts.Get(guid) != null && type.Equals(NovaElementType.CONDITION)) return RegisteredConditionContexts.Get(guid);
            else if (RegisteredEventContexts.Get(guid) != null && type.Equals(NovaElementType.EVENT)) return RegisteredEventContexts.Get(guid);
            else return null;
        }
    }
}
