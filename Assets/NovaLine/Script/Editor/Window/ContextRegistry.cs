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
        public static Dictionary<string, INovaContext> RegisteredContexts { get; } = new();

        public static FlowchartContext RegisteredFlowchartContext =>
            RootGraphViewNodeContext as FlowchartContext;

        public static ListExt<IGraphViewNodeContext> LoadedGraphViewContexts { get; } = new();

        public static IGraphViewNodeContext LastGraphViewNodeContext =>
            LoadedGraphViewContexts.Count > 1 ? LoadedGraphViewContexts[1] : null;

        public static IGraphViewNodeContext CurrentGraphViewNodeContext =>
            LoadedGraphViewContexts.Count > 0 ? LoadedGraphViewContexts[0] : null;

        public static IGraphViewNodeContext RootGraphViewNodeContext =>
            LoadedGraphViewContexts.Count > 0 ? LoadedGraphViewContexts[^1] : null;

        public static void RegisterContext(INovaContext graphViewNodeContext)
        {
            if (graphViewNodeContext?.LinkedData == null) return;

            switch (graphViewNodeContext.LinkedData)
            {
                case IGraphViewNodeData graphViewNodeData:
                {
                    var nodeDataList = graphViewNodeData.NodeDataList;
                    if (nodeDataList != null && nodeDataList.Count > 0)
                    {
                        foreach (var nodeData in graphViewNodeData.NodeDataList)
                        {
                            var childContext = CreateContextByType(nodeData, nodeData.LinkedElement.Type);
                            RegisterContext(childContext);
                        }
                    }

                    var edgeDataList = graphViewNodeData.EdgeDataList;
                    if (edgeDataList != null && edgeDataList.Count > 0)
                    {
                        foreach (var edgeData in graphViewNodeData.EdgeDataList)
                        {
                            var edgeContext = new EdgeContext(edgeData);
                            RegisterContext(edgeContext);
                        }
                    }

                    if (graphViewNodeData is IAroundConditionData hasConditionData)
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
                    if (edgeData is NodeEdgeData nodeEdgeData && nodeEdgeData.SwitchConditionData != null)
                    {
                        var conditionContext = new ConditionContext(nodeEdgeData.SwitchConditionData);
                        RegisterContext(conditionContext);
                    }

                    break;
                }
            }

            RegisteredContexts.TryAdd(graphViewNodeContext.GUID, graphViewNodeContext);
        }

        public static void UnregisterContext(string contextGUID, NovaElementType type)
        {
            UnregisterContext(GetContext(contextGUID, type));
        }

        public static void UnregisterContext(INovaContext graphViewNodeContext)
        {
            if (graphViewNodeContext?.LinkedData == null) return;

            switch (graphViewNodeContext.LinkedData)
            {
                case IGraphViewNodeData graphViewNodeData:
                {
                    if (graphViewNodeData.NodeDataList != null && graphViewNodeData.NodeDataList.Count > 0)
                    {
                        foreach (var nodeData in graphViewNodeData.NodeDataList)
                        {
                            if (nodeData?.LinkedElement == null) continue;
                            var childContext = GetContext(nodeData.GUID, nodeData.LinkedElement.Type);
                            UnregisterContext(childContext);
                        }
                    }

                    if (graphViewNodeData.EdgeDataList != null && graphViewNodeData.EdgeDataList.Count > 0)
                    {
                        foreach (var edgeData in graphViewNodeData.EdgeDataList)
                        {
                            UnregisterContext(edgeData.GUID, NovaElementType.Switcher);
                        }
                    }

                    if (graphViewNodeData is IAroundConditionData hasConditionData)
                    {
                        UnregisterContext(hasConditionData.ConditionBeforeInvokeData?.GUID, NovaElementType.Condition);
                        UnregisterContext(hasConditionData.ConditionAfterInvokeData?.GUID, NovaElementType.Condition);
                    }

                    break;
                }
                case IEdgeData edgeData:
                {
                    if (edgeData is NodeEdgeData nodeEdgeData && nodeEdgeData.SwitchConditionData != null)
                    {
                        UnregisterContext(nodeEdgeData.SwitchConditionData.GUID, NovaElementType.Condition);
                    }

                    break;
                }
            }

            RegisteredContexts.Remove(graphViewNodeContext.GUID);
        }

        public static void ClearContexts()
        {
            foreach (var context in RegisteredContexts.Values.OfType<IGraphViewNodeContext>().Distinct().ToList())
            {
                context?.DisposeGraphView();
            }

            RegisteredContexts.Clear();
            LoadedGraphViewContexts.Clear();
        }

        public static INovaContext GetContext(GraphNode graphNode, NovaElementType type)
        {
            if (graphNode == null) return null;
            var guid = graphNode.GUID ?? graphNode.LinkedElement?.GUID;
            return GetContext(guid, type);
        }

        public static INovaContext GetContext(string guid, NovaElementType type)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            return RegisteredContexts.TryGetValue(guid, out var ctx) && ctx.Type == type ? ctx : null;
        }

        public static INovaContext CreateContextByType(INovaData linkedData, NovaElementType type)
        {
            return type switch
            {
                NovaElementType.Flowchart => new FlowchartContext(linkedData as FlowchartData),
                NovaElementType.Node => new NodeContext(linkedData as NodeData),
                NovaElementType.Action => new ActionContext(linkedData as ActionData),
                NovaElementType.Condition => new ConditionContext(linkedData as ConditionData),
                NovaElementType.Event => new EventContext(linkedData as EventData),
                NovaElementType.Switcher => new EdgeContext(linkedData as IEdgeData),
                _ => null
            };
        }
    }
}
