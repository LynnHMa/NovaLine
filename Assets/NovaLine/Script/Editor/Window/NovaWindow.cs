using NovaLine.Script.Editor.File;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static NovaLine.Script.Editor.Window.ContextRegistry;
using static NovaLine.Script.Editor.Utils.InspectorHelper;
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
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.delayCall += TryRestoreAfterReload;
        }
        private void OnDisable()
        {
            Undo.willFlushUndoRecord -= OnInspectorObjValueChange;
            Undo.undoRedoPerformed -= OnInspectorObjValueChange;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            EditorApplication.delayCall -= TryRestoreAfterReload;

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
        private void TryRestoreAfterReload()
        {
            if (CurrentGraphViewNodeContext != null) return;
            EditorFileManager.RestoreAfterDomainReload();
        }
        private void OnBeforeAssemblyReload()
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
            var graphView = (GraphView)context.GraphView;
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
                context.GraphView.SetBackButtonVisible(false);
                context.LinkedData.registerLinkedElement();
            }
            else if(context is not FlowchartNodeContext)
            {
                context.GraphView.SetBackButtonVisible(true);
                context.GraphView.OnRequestBackToParent = ExitGraphView;
            }

            Instance.rootVisualElement.Clear();

            Debug.Log($"Loaded {context.LinkedData.nodeDataList.Count} nodes , {context.LinkedData.edgeDataList.Count} edges! Data name: {context.LinkedData.name}");

            if (isExiting)
            {
                //Reselect the graph node or edge in parent graph view.
                var presentContext = CurrentGraphViewNodeContext;
                if (presentContext != null)
                {
                    if(presentContext is ConditionContext conditionContext)
                    {
                        if (!context.GraphView.SelectGraphNode(conditionContext.LinkedData.linkedElement.Parent.Guid))
                        {
                            context.GraphView.SelectGraphEdge(conditionContext.LinkedData.linkedElement.Parent.Guid);
                        }
                    }
                    else
                    {
                        if (!context.GraphView.SelectGraphNode(presentContext.Guid))
                        {
                            context.GraphView.SelectGraphEdge(presentContext.Guid);
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
                    LoadedGraphViewContexts.RemoveAll(toRemove => toRemove.Type == NovaElementType.CONDITION);
                }

                //Let's load new context :)
                LoadedGraphViewContexts.Insert(0, context);

                SelectedGraphEdge = null;
                SelectedGraphNode = null;

                //Update linked element of current context.
                var linkedContextElement = context.LinkedData.linkedElement;
                linkedContextElement?.ShowInInspector();

                //Init the context.
                context.Draw();
            }
        
            //Add this graph view to opened window,make sure u can see it.
            Instance.rootVisualElement.Add(graphView);
            graphView.StretchToParentSize();

            EditorFileManager.CurrentContextGuid = context.Guid;
            EditorFileManager.CurrentContextType = context.Type;
        
            UpdateScope.RequireUpdate();
        }
        public static void UpdateContext()
        {
            CurrentGraphViewNodeContext?.GraphView?.Update();

            if (Instance == null) return;

            Instance.titleContent.text = CurrentGraphViewNodeContext?.Title;
        }
        #endregion
    }
}