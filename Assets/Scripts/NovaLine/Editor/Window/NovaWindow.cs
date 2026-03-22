using NovaLine.Editor.File;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Window;
using NovaLine.Editor.Window.Context;
using NovaLine.Element;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static NovaLine.Editor.Window.WindowContextRegistry;
using static NovaLine.Editor.Window.InspectorHelper;
public class NovaWindow : EditorWindow
{
    public static NovaWindow Instance { get; set; }
    public static GraphNode SelectedGraphNode { get; set; }
    public static IGraphEdge SelectedGraphEdge { get; set; }

    private void OnEnable()
    {
        Instance = this;
        Undo.willFlushUndoRecord += OnInspectorObjValueChange;
        Undo.undoRedoPerformed += OnInspectorObjValueChange;
        AssemblyReloadEvents.beforeAssemblyReload += onBeforeAssemblyReload;
        EditorApplication.delayCall += tryRestoreAfterReload;
    }
    private void OnDisable()
    {
        Undo.willFlushUndoRecord -= OnInspectorObjValueChange;
        Undo.undoRedoPerformed -= OnInspectorObjValueChange;
        AssemblyReloadEvents.beforeAssemblyReload -= onBeforeAssemblyReload;
        EditorApplication.delayCall -= tryRestoreAfterReload;

        if (!EditorApplication.isCompiling)
        {
            EditorFileManager.SaveGraphWindowData();
        }

        EditorApplication.delayCall += () =>
        {
            SelectedGraphNode = null;
            SelectedGraphEdge = null;
            InspectorHelper.ShowInInspector(null);
            ClearContexts();
        };
    }
    private void tryRestoreAfterReload()
    {
        if (CurrentGraphViewContext != null) return;
        EditorFileManager.RestoreAfterDomainReload();
    }
    private void onBeforeAssemblyReload()
    {
        if (!EditorApplication.isCompiling)
        {
            EditorFileManager.SaveGraphWindowData();
        }
    }

    #region STATIC
    public static void CreateGraphWindow()
    {
        Instance = GetWindow<NovaWindow>("Empty Window");
    }

    [MenuItem("NovaLine/New Flowchart")]
    public static void NewFlowchartInWindow()
    {
        var newDataAsset = EditorFileManager.CreateNewFlowchartAsset();
        if (newDataAsset == null) return;
        var newFlowchartData = newDataAsset.data;
        var newContext = new FlowchartContext(newFlowchartData);
        EditorApplication.delayCall += () =>
        {
            LoadContextInWindow(newContext);
        };
    }

    [MenuItem("NovaLine/Exit Child Graph View")]
    public static bool ExitGraphView()
    {
        EditorFileManager.SaveGraphWindowData();
        if (CurrentGraphViewContext == null || LastGraphViewContext == null)
        {
            return false;
        }
        LoadContextInWindow(LastGraphViewContext, true);
        return true;
    }

    public static void LoadContextInWindow(IGraphViewContext context, bool isExiting = false)
    {
        var graphView = (GraphView)context.graphView;
        if (graphView == null) return;

        if (Instance == null) CreateGraphWindow();

        //Register flowchart and its children
        if (context is FlowchartContext flowchartContext && !isExiting)
        {
            ClearContexts();
            if(RegisteredFlowchartContext != null)
            {
                UnregisterContext(RegisteredFlowchartContext);
            }
            RegisterContext(flowchartContext);
        }

        Instance.rootVisualElement.Clear();

        Debug.Log($"Loaded {context.linkedData.nodeDatas.Count} nodes , {context.linkedData.edgeDatas.Count} edges! Data name: {context.linkedData.name}");

        if (isExiting)
        {
            //Reselect the graph node or edge in parent graph view.
            var presentContext = CurrentGraphViewContext;
            if (presentContext != null)
            {
                if(presentContext is ConditionContext conditionContext)
                {
                    if (!context.graphView.selectGraphNode(conditionContext.linkedData.linkedElement.parent.guid))
                    {
                        context.graphView.selectGraphEdge(conditionContext.linkedData.linkedElement.parent.guid);
                    }
                }
                else
                {
                    if (!context.graphView.selectGraphNode(presentContext.guid))
                    {
                        context.graphView.selectGraphEdge(presentContext.guid);
                    }
                }

                //Remove present context from loaded list.
                LoadedGraphViewContexts.RemoveAt(0);
            }
        }
        else
        {
            //Multi condition contexts are not allowed,so we need to clean all of other them.
            if (context is ConditionContext)
            {
                LoadedGraphViewContexts.RemoveAll(context => context.type == NovaElementType.CONDITION);
            }

            //Let's load new context :)
            LoadedGraphViewContexts.Insert(0, context);

            SelectedGraphEdge = null;
            SelectedGraphNode = null;

            //Update linked element of current context.
            var linkedContextElement = context.linkedData.linkedElement;
            linkedContextElement?.ShowInInspector();

            //Init the context.
            context.draw();
        }

        //Update all of graph elements.
        context.graphView.update();

        //Add this graph view to opened window,make sure u can see it.
        Instance.rootVisualElement.Add(graphView);
        graphView.StretchToParentSize();

        UpdateContext();
        EditorFileManager.CurrentContextGuid = context.guid;
        EditorFileManager.CurrentContextType = context.type;
    }
    public static void UpdateContext()
    {
        CurrentGraphViewContext?.graphView?.update();

        if (Instance == null) return;

        Instance.titleContent.text = CurrentGraphViewContext?.title;
    }
    #endregion
}