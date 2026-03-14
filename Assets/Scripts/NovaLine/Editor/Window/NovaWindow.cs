using NovaLine.Editor.File;
using NovaLine.Editor.Graph.Data.NodeGraphView;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Window.Context;
using NovaLine.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NovaWindow : EditorWindow
{
    private static NovaWindow Instance;
    public static GraphNode SelectedGraphNode { get; set; }
    public static IGraphEdge SelectedGraphEdge { get; set; }

    public FlowchartContext registeredFlowchartContext { get; set; }
    public EList<NodeContext> registeredNodeContexts { get; set; } = new();
    public EList<ActionContext> registeredActionContexts { get; set; } = new();
    public EList<ConditionContext> registeredConditionContexts {  get; set; } = new();
    public EList<EventContext> registeredEventContexts { get; set; } = new();

    public EList<IGraphViewContext> loadedGraphViewContexts { get; set; } = new();
    public IGraphViewContext lastGraphViewContext => loadedGraphViewContexts.Count > 1 ? loadedGraphViewContexts[1] : null;
    public IGraphViewContext currentGraphViewContext => loadedGraphViewContexts.Count > 0 ? loadedGraphViewContexts[0] : null;

    private void OnEnable()
    {
        Instance = this;
        Undo.willFlushUndoRecord += onInspectorObjValueChange;
        Undo.undoRedoPerformed += onInspectorObjValueChange;
        AssemblyReloadEvents.beforeAssemblyReload += onBeforeAssemblyReload;
        EditorApplication.delayCall += tryRestoreAfterReload;
    }
    private void OnDisable()
    {
        Undo.willFlushUndoRecord -= onInspectorObjValueChange;
        Undo.undoRedoPerformed -= onInspectorObjValueChange;
        AssemblyReloadEvents.beforeAssemblyReload -= onBeforeAssemblyReload;
        EditorApplication.delayCall -= tryRestoreAfterReload;

        if (!EditorApplication.isCompiling)
        {
            NovaFileManager.SaveGraphWindowData();
        }

        EditorApplication.delayCall += () =>
        {
            SelectedGraphNode = null;
            SelectedGraphEdge = null;
            loadedGraphViewContexts.Clear();
        };
    }
    private void tryRestoreAfterReload()
    {
        if (currentGraphViewContext != null) return;
        NovaFileManager.RestoreAfterDomainReload();
    }
    private void onBeforeAssemblyReload()
    {
        if (!EditorApplication.isCompiling)
        {
            NovaFileManager.SaveGraphWindowData();
        }
    }
    private void onInspectorObjValueChange()
    {
        currentGraphViewContext?.graphView.update();
    }
    protected void registerContext(IGraphViewContext graphViewContext)
    {
        if (graphViewContext is FlowchartContext flowchartContext)
        {
            registeredFlowchartContext = flowchartContext;
            foreach(var nodeData in flowchartContext.linkedData.nodeDatas)
            {
                var nodeContext = new NodeContext(nodeData);
                registerContext(nodeContext);
            }
            foreach (var nodeEdgeData in flowchartContext.linkedData.edgeDatas)
            {
                var edgeConditionContext = new ConditionContext(nodeEdgeData.switchConditionData);
                registerContext(edgeConditionContext);
            }
        }
        else if (graphViewContext is NodeContext nodeContext)
        {
            foreach (var actionData in nodeContext.linkedData.nodeDatas)
            {
                var actionContext = new ActionContext(actionData);
                registerContext(actionContext);
            }

            var conditionBeforeNodeInvoke = nodeContext.linkedData?.conditionBeforeInvokeData;
            var conditionAfterNodeInvoke = nodeContext.linkedData?.conditionAfterInvokeData;
            if(conditionBeforeNodeInvoke != null)
            {
                var conditionBeforeInvokeContext = new ConditionContext(conditionBeforeNodeInvoke);
                registerContext(conditionBeforeInvokeContext);
            }
            if(conditionAfterNodeInvoke != null)
            {
                var conditionAfterInvokeContext = new ConditionContext(conditionAfterNodeInvoke);
                registerContext(conditionAfterInvokeContext);
            }

            registeredNodeContexts.Add(nodeContext);
        }
        else if (graphViewContext is ActionContext actionContext)
        {
            var conditionBeforeActionInvoke = actionContext.linkedData?.conditionBeforeInvokeData;
            var conditionAfterActionInvoke = actionContext.linkedData?.conditionAfterInvokeData;
            if (conditionBeforeActionInvoke != null)
            {
                var conditionBeforeInvokeContext = new ConditionContext(conditionBeforeActionInvoke);
                registerContext(conditionBeforeInvokeContext);
            }
            if (conditionAfterActionInvoke != null)
            {
                var conditionAfterInvokeContext = new ConditionContext(conditionAfterActionInvoke);
                registerContext(conditionAfterInvokeContext);
            }

            registeredActionContexts.Add(actionContext);
        }
        else if(graphViewContext is ConditionContext conditionContext)
        {
            foreach (var eventData in conditionContext.linkedData.nodeDatas)
            {
                var eventContext = new EventContext(eventData);
                registerContext(eventContext);
            }
            Debug.Log("registered : " + conditionContext.guid);
            registeredConditionContexts.Add(conditionContext);
        }
        else if(graphViewContext is EventContext eventContext)
        {
            registeredEventContexts.Add(eventContext);
        }
    }
    protected void unregisterContext(string contextGuid)
    {
        unregisterContext(getContext(contextGuid));
    }
    protected void unregisterContext(IGraphViewContext graphViewContext)
    {
        if (graphViewContext is FlowchartContext flowchartContext)
        {
            foreach (var nodeData in flowchartContext.linkedData.nodeDatas)
            {
                unregisterContext(nodeData.guid);
            }
            foreach (var nodeEdgeData in flowchartContext.linkedData.edgeDatas)
            {
                unregisterContext(nodeEdgeData.guid);
            }
            registeredFlowchartContext = null;
        }
        else if (graphViewContext is NodeContext nodeContext)
        {
            foreach (var actionData in nodeContext.linkedData.nodeDatas)
            {
                unregisterContext(actionData.guid);
            }
            unregisterContext(nodeContext.linkedData?.conditionBeforeInvokeData?.guid);
            unregisterContext(nodeContext.linkedData?.conditionAfterInvokeData?.guid);
            registeredNodeContexts.Remove(nodeContext);
        }
        else if(graphViewContext is ActionContext actionContext)
        {
            unregisterContext(actionContext.linkedData?.conditionBeforeInvokeData?.guid);
            unregisterContext(actionContext.linkedData?.conditionAfterInvokeData?.guid);
            registeredActionContexts.Remove(actionContext);
        }
        else if(graphViewContext is ConditionContext conditionContext)
        {
            foreach (var eventData in conditionContext.linkedData.nodeDatas)
            {
                unregisterContext(eventData.guid);
            }
            registeredConditionContexts.Remove(conditionContext);
        }
        else if(graphViewContext is EventContext eventContext)
        {
            registeredEventContexts.Remove(eventContext);
        }
    }
    protected IGraphViewContext getContext(GraphNode graphNode)
    {
        return getContext(graphNode?.guid);
    }
    protected IGraphViewContext getContext(string guid)
    {
        if (registeredFlowchartContext.guid.Equals(guid)) return registeredFlowchartContext;
        else if (registeredNodeContexts.Get(guid) != null) return registeredNodeContexts.Get(guid);
        else if (registeredActionContexts.Get(guid) != null) return registeredActionContexts.Get(guid);
        else if(registeredConditionContexts.Get(guid) != null) return registeredConditionContexts.Get(guid);
        else if(registeredEventContexts.Get(guid) != null) return registeredEventContexts.Get(guid);
        else return null;
    }

    #region STATIC
    public static void CreateGraphWindow()
    {
        Instance = GetWindow<NovaWindow>("Empty Window");
    }
    public static NovaWindow GetMainWindowInstance()
    {
        return Instance;
    }

    [MenuItem("NovaLine/New Flowchart")]
    public static void NewFlowchartInWindow()
    {
        var newDataAsset = NovaFileManager.CreateNewFlowchartAsset();
        if (newDataAsset == null) return;
        var newFlowchartData = newDataAsset.data;
        var newContext = new FlowchartContext(newFlowchartData);
        EditorApplication.delayCall += () =>
        {
            LoadContextInWindow(newContext);
        };
    }

    [MenuItem("NovaLine/Exit Child Graph View")]
    public static bool ExitChildGraphView()
    {
        NovaFileManager.SaveGraphWindowData();
        var currentWindow = GetMainWindowInstance();
        if (currentWindow.currentGraphViewContext == null || currentWindow.lastGraphViewContext == null)
        {
            return false;
        }
        LoadContextInWindow(currentWindow.lastGraphViewContext, true);
        return true;
    }

    public static void LoadContextInWindow(IGraphViewContext context, bool isExiting = false)
    {
        if(GetMainWindowInstance() == null) CreateGraphWindow();
        var window = GetMainWindowInstance();

        //Register flowchart and its children
        if (context is FlowchartContext flowchartContext && !isExiting)
        {
            window.loadedGraphViewContexts.Clear();
            var presentFlowchartContext = window.registeredFlowchartContext;
            if(presentFlowchartContext != null)
            {
                window.unregisterContext(presentFlowchartContext);
            }
            window.registerContext(flowchartContext);
        }

        window.rootVisualElement.Clear();

        Debug.Log($"Loaded {context.linkedData.nodeDatas.Count} nodes , {context.linkedData.edgeDatas.Count} edges! Data name: " + context.linkedData.name);

        if (isExiting)
        {
            window.loadedGraphViewContexts.RemoveAt(0);
        }
        else
        {
            window.loadedGraphViewContexts.Insert(0, context);
            context.draw();
        }

        var graphView = (GraphView)context.graphView;
        if (graphView == null) return;

        window.rootVisualElement.Add(graphView);
        graphView.StretchToParentSize();

        UpdateWindowTitle();
        NovaFileManager.CurrentContextGuid = context.guid;
    }

    public static void UpdateWindowTitle()
    {
        var window = GetMainWindowInstance();

        if (window == null) return;

        window.titleContent.text = window.currentGraphViewContext?.title;
    }

    public static void RegisterContext(IGraphViewContext context)
    {
        var window = GetMainWindowInstance();

        if (window == null) return;

        window.registerContext(context);
    }

    public static void UnregisterContext(string contextGuid)
    {
        var window = GetMainWindowInstance();

        if (window == null) return;

        window.unregisterContext(contextGuid);
    }

    public static void UnregisterContext(IGraphViewContext context)
    {
        var window = GetMainWindowInstance();

        if (window == null) return;

        window.unregisterContext(context);
    }

    public static IGraphViewContext GetContext(GraphNode graphNode)
    {
        var window = GetMainWindowInstance();

        if (window == null) return null;

        return window.getContext(graphNode);
    }

    public static IGraphViewContext GetContext(string contextGuid)
    {
        var window = GetMainWindowInstance();

        if (window == null) return null;

        return window.getContext(contextGuid);
    }

    public static void UpdateContext()
    {
        var window = GetMainWindowInstance();

        if (window == null) return;

        window.currentGraphViewContext.graphView.update();

        UpdateWindowTitle();
    }
    #endregion
}