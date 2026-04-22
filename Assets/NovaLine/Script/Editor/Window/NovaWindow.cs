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

        public static GraphNode SelectedGraphNode =>
            CurrentGraphViewNodeContext?.GraphView?.GetExistingGraphNode(InspectorNovaElementWrapper?.SelectedElementGuid,1);
        public static IGraphEdge SelectedGraphEdge =>
            CurrentGraphViewNodeContext?.GraphView?.GetExistingGraphEdge(InspectorNovaElementWrapper?.SelectedElementGuid,1);

        private void OnEnable()
        {
            Instance = this;
            Undo.willFlushUndoRecord += OnInspectorObjValueChange;
            Undo.undoRedoPerformed += OnInspectorObjValueChange;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.delayCall += TryRestoreAfterReload;
            
            EditorApplication.delayCall += () =>
            {
                if (CurrentGraphViewNodeContext != null)
                {
                    LoadContextInWindow(CurrentGraphViewNodeContext);
                }
                else
                {
                    TryRestoreAfterReload();
                }
            };
            
            //因为Unity迷惑的保护机制，导致一些快捷键无法添加，这里将强制劫持
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnWindowKeyDown, TrickleDown.TrickleDown);
        }
        private void OnDisable()
        {
            Undo.willFlushUndoRecord -= OnInspectorObjValueChange;
            Undo.undoRedoPerformed -= OnInspectorObjValueChange;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            EditorApplication.delayCall -= TryRestoreAfterReload;

            if (!EditorApplication.isCompiling)
            {
                EditorFileManager.SaveCurrentGraphViewNodeData();
            }
        }
        

        #region STATIC
        public static void CreateGraphWindow()
        {
            Instance = GetWindow<NovaWindow>("Nova Window");
        }
        
        public static void ExitGraphView()
        {
            SaveScope.RequireSave();
            if (CurrentGraphViewNodeContext == null || LastGraphViewNodeContext == null)
            {
                return;
            }
            LoadContextInWindow(LastGraphViewNodeContext, true);
        }

        public static void LoadContextInWindow(IGraphViewNodeContext context, bool isBackToParent = false)
        {
            if (context == null) return;
            if (Instance == null) CreateGraphWindow();
            
            Instance.rootVisualElement.Clear();

            if (isBackToParent)
            {
                //Reselect the graph node or edge in parent graph view.
                var parentContext = CurrentGraphViewNodeContext;
                if (parentContext != null)
                {
                    if(parentContext is ConditionContext conditionContext)
                    {
                        if (!context.GraphView.SelectGraphNode(conditionContext.LinkedData.LinkedElement.Parent.Guid))
                        {
                            context.GraphView.SelectGraphEdge(conditionContext.LinkedData.LinkedElement.Parent.Guid);
                        }
                    }
                    else
                    {
                        if (!context.GraphView.SelectGraphNode(parentContext.Guid))
                        {
                            context.GraphView.SelectGraphEdge(parentContext.Guid);
                        }
                    }

                    //Remove present context from loaded list.
                    LoadedGraphViewContexts.RemoveAt(0);
                }
            }
            else
            {
                //Congeneric contexts are not allowed,so we need to clean all other them before drawing.
                LoadedGraphViewContexts.RemoveAll(toRemove => toRemove.Type == context.Type);
                LoadedGraphViewContexts.Insert(0, context);
                
                //Update linked element of current context.
                var linkedElement = context.LinkedData.LinkedElement;
                linkedElement?.ShowInInspector();

                //Draw the context.
                context.Draw();
            }

            //Recording context info to be used to restore after reloading domain
            EditorFileManager.CurrentContextGuid = context.Guid;
            EditorFileManager.CurrentContextType = context.Type;
            
            //Show back button
            var isRootContext = context.Guid.Equals(RootGraphViewNodeContext.Guid);
            if(isRootContext)
            {
                context.GraphView.SetBackButtonVisible(false);
            }
            else
            {
                context.GraphView.SetBackButtonVisible(true);
                context.GraphView.OnRequestBackToParent = ExitGraphView;
            }
            
            //Add this graph view to opened window,make sure u can see it.
            var unityGraphView = (GraphView)context.GraphView;
            if (unityGraphView == null) return;
            Instance.rootVisualElement.Add(unityGraphView);
            unityGraphView.StretchToParentSize();
            unityGraphView.Focus();
            UpdateScope.RequireUpdate();
        }
        public static void LoadConditionContextDirect(Condition targetCondition, string fallbackName)
        {
            if (targetCondition == null)
            {
                Debug.LogWarning("Can't load condition context ,because Condition is null!");
                return;
            }

            var context = GetContext(targetCondition.Guid, NovaElementType.Condition);
            if (context is ConditionContext conditionContext)
            {
                var actualConditionName = targetCondition.name;

                if (string.IsNullOrEmpty(actualConditionName))
                {
                    actualConditionName = fallbackName;
                }

                targetCondition.name = actualConditionName;
                LoadContextInWindow(conditionContext);
            }
        }
        public static void UpdateContext()
        {
            CurrentGraphViewNodeContext?.GraphView?.Update();

            if (Instance == null) return;

            Instance.titleContent.text = CurrentGraphViewNodeContext?.Title;
        }
        private static void OnWindowKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape)
            {
                if (CurrentGraphViewNodeContext != null && LastGraphViewNodeContext != null)
                {
                    evt.StopPropagation();
                    evt.PreventDefault();
                    ExitGraphView();
                }
            }
        }
        private static void TryRestoreAfterReload()
        {
            if (CurrentGraphViewNodeContext != null) return;
            EditorFileManager.RestoreAfterDomainReload();
        }
        private static void OnBeforeAssemblyReload()
        {
            if (!EditorApplication.isCompiling)
            {
                EditorFileManager.SaveCurrentGraphViewNodeData();
            }
        }
        #endregion
    }
}