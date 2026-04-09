using NovaLine.Script.Editor.File;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static NovaLine.Script.Editor.Window.ContextRegistry;
using static NovaLine.Script.Editor.Window.InspectorHelper;
namespace NovaLine.Script.Editor.Window
{
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
            if (CurrentGraphViewNodeContext != null) return;
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
            var newContext = new FlowchartNodeContext(newFlowchartData);
            EditorApplication.delayCall += () =>
            {
                LoadContextInWindow(newContext);
            };
        }
        
        public static void ExitGraphView()
        {
            EditorFileManager.SaveGraphWindowData();
            if (CurrentGraphViewNodeContext == null || LastGraphViewNodeContext == null)
            {
                return;
            }
            LoadContextInWindow(LastGraphViewNodeContext, true);
        }

        public static void LoadContextInWindow(IGraphViewNodeContext context, bool isExiting = false)
        {
            var graphView = (GraphView)context.graphView;
            if (graphView == null) return;

            if (Instance == null) CreateGraphWindow();

            //Register flowchart and its children
            if (context is FlowchartNodeContext flowchartContext && !isExiting)
            {
                ClearContexts();
                if(RegisteredFlowchartNodeContext != null)
                {
                    UnregisterContext(RegisteredFlowchartNodeContext);
                }
                RegisterContext(flowchartContext);
                context.graphView.setBackButtonVisible(false);
                context.linkedData.registerLinkedElement();
            }
            else if(context is not FlowchartNodeContext)
            {
                context.graphView.setBackButtonVisible(true);
                context.graphView.OnRequestBackToParent = ExitGraphView;
            }

            Instance.rootVisualElement.Clear();

            Debug.Log($"Loaded {context.linkedData.nodeDataList.Count} nodes , {context.linkedData.edgeDataList.Count} edges! Data name: {context.linkedData.name}");

            if (isExiting)
            {
                //Reselect the graph node or edge in parent graph view.
                var presentContext = CurrentGraphViewNodeContext;
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
                //Multi condition contexts are not allowed,so we need to clean all other them.
                if (context is ConditionContext)
                {
                    LoadedGraphViewContexts.RemoveAll(toRemove => toRemove.type == NovaElementType.CONDITION);
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
        
            //Add this graph view to opened window,make sure u can see it.
            Instance.rootVisualElement.Add(graphView);
            graphView.StretchToParentSize();

            EditorFileManager.CurrentContextGuid = context.guid;
            EditorFileManager.CurrentContextType = context.type;
        
            UpdateScope.RequireUpdate();
        }
        public static void UpdateContext()
        {
            CurrentGraphViewNodeContext?.graphView?.update();

            if (Instance == null) return;

            Instance.titleContent.text = CurrentGraphViewNodeContext?.title;
        }
        #endregion
    }
}