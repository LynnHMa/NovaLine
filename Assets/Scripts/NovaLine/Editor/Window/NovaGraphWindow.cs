using NovaLine.Editor.File;
using NovaLine.Editor.Graph.Data;
using NovaLine.Editor.Graph.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class NovaGraphWindow : EditorWindow
{
    private static NovaGraphWindow Instance;

    public OpenedNovaGraphView currentOpenedGraphView => openedGraphViews?[0];
    public OpenedNovaGraphView lastOpenedGraphView => openedGraphViews.Count < 2 ? null : openedGraphViews?[1];
    public OpenedNovaGraphView rootOpenedGraphView => openedGraphViews.Count == 0 ? null : openedGraphViews?[openedGraphViews.Count - 1];
    public List<OpenedNovaGraphView> openedGraphViews { get; set; } = new();

    [MenuItem("NovaLine/Open Flowchart Editor")]
    public static void createGraphWindow()
    {
        var currentWindow = getMainWindowInstance();
    }
    [MenuItem("NovaLine/New Flowchart")]
    public static void newFlowchartInWindow()
    {
        var newData = NovaFileManager.createAndLoadNewFlowchartDataFile();
        if (newData == null) return;
        var newFlowchart = newData.to();
        loadFlowchartInWindow(newData,new FlowchartGraphView(newFlowchart));
    }
    [MenuItem("NovaLine/Exit Child Graph View")]
    public static void exitChildGraphView()
    {
        NovaFileManager.saveGraphWindowData();
        EditorApplication.delayCall += () =>
        {
            var currentWindow = getMainWindowInstance();
            if (currentWindow.currentOpenedGraphView == null || currentWindow.lastOpenedGraphView == null) return;
            loadFlowchartInWindow(currentWindow.lastOpenedGraphView,true);
        };
    }

    public static void loadFlowchartInWindow(IGraphViewNodeData data, INovaGraphView iGraphView, bool isExiting = false)
    {
        loadFlowchartInWindow(new OpenedNovaGraphView(iGraphView,data), isExiting);
    }
    public static void loadFlowchartInWindow(OpenedNovaGraphView opened, bool isExiting = false)
    {
        var currentWindow = getMainWindowInstance();

        currentWindow.rootVisualElement.Clear();

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
        if (currentWindow.currentOpenedGraphView.graphView is GraphView graphView)
        {
            currentWindow.rootVisualElement?.Add(graphView);
            graphView.StretchToParentSize();
        }
    }

    public static NovaGraphWindow getMainWindowInstance()
    {
        if(Instance == null)
        {
            Instance = GetWindow<NovaGraphWindow>("Empty Window");
        }
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