using NovaLine.Editor.File;
using NovaLine.Editor.Graph.Data;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NovaGraphWindow : EditorWindow
{
    private static NovaGraphWindow Instance;
    public static GraphNode nodeInInspector { get; set; }

    public static IGraphEdge edgeInInspector { get; set; }

    public OpenedNovaGraphView currentOpenedGraphView => openedGraphViews?[0];
    public OpenedNovaGraphView lastOpenedGraphView => openedGraphViews.Count < 2 ? null : openedGraphViews?[1];
    public OpenedNovaGraphView rootOpenedGraphView => openedGraphViews.Count == 0 ? null : openedGraphViews?[openedGraphViews.Count - 1];
    public List<OpenedNovaGraphView> openedGraphViews { get; set; } = new();

    private void OnEnable()
    {
        Undo.willFlushUndoRecord += onInspectorObjValueChange;
        Undo.undoRedoPerformed += onInspectorObjValueChange;
    }
    private void OnDisable()
    {
        Undo.willFlushUndoRecord -= onInspectorObjValueChange;
        Undo.undoRedoPerformed -= onInspectorObjValueChange;
    }
    private void OnDestroy()
    {
        NovaFileManager.saveGraphWindowData();
        EditorApplication.delayCall += () =>
        {
            nodeInInspector = null;
            edgeInInspector = null;
            openedGraphViews.Clear();
        };
    }
    private void onInspectorObjValueChange()
    {
        var currentView = currentOpenedGraphView?.graphView;
        currentView.updateAllGraphElements();
    }

    public static void createGraphWindow()
    {
        Instance = GetWindow<NovaGraphWindow>("Empty Window");
    }
    [MenuItem("NovaLine/New Flowchart")]
    public static void newFlowchartInWindow()
    {
        var newDataAsset = NovaFileManager.createAndLoadNewFlowchartDataFile();
        if (newDataAsset == null) return;
        var newData = newDataAsset.data;
        var newFlowchart = newData.linkedElement;
        EditorApplication.delayCall += () =>
        {
            loadFlowchartInWindow(newData, new FlowchartGraphView(newFlowchart));
        };
    }
    [MenuItem("NovaLine/Exit Child Graph View")]
    public static void exitChildGraphView()
    {
        NovaFileManager.saveGraphWindowData();
        EditorApplication.delayCall += () =>
        {
            var currentWindow = getMainWindowInstance();
            if (currentWindow.currentOpenedGraphView == null || currentWindow.lastOpenedGraphView == null) return;
            loadFlowchartInWindow(currentWindow.lastOpenedGraphView, true);
        };
    }

    public static void loadFlowchartInWindow(IGraphViewNodeData data, INovaGraphView iGraphView, bool isExiting = false)
    {
        loadFlowchartInWindow(new OpenedNovaGraphView(iGraphView,data), isExiting);
    }
    public static void loadFlowchartInWindow(OpenedNovaGraphView opened, bool isExiting = false)
    {
        if(getMainWindowInstance() == null) createGraphWindow();
        var currentWindow = getMainWindowInstance();

        currentWindow.rootVisualElement.Clear();

        if (opened.graphView is not GraphView graphView) return;

        if (isExiting)
        {
            currentWindow.openedGraphViews.RemoveAt(0);
        }
        else
        {
            currentWindow.openedGraphViews.Insert(0, opened);
            opened.linkedData.draw(opened.graphView);
        }
        currentWindow.titleContent.text = opened.graphView?.getName();
        currentWindow.rootVisualElement?.Add(graphView);
        graphView.StretchToParentSize();
        opened.graphView.updateAllGraphElements();
    }

    public static NovaGraphWindow getMainWindowInstance()
    {
        return Instance;
    }
    public class OpenedNovaGraphView
    {
        public OpenedNovaGraphView(INovaGraphView graphView, IGraphViewNodeData linkedData)
        {
            this.graphView = graphView;
            this.linkedData = linkedData;
        }
        public INovaGraphView graphView { get; set; }
        public IGraphViewNodeData linkedData { get; set; }

    }
}