using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NovaLine.Script.Data;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window;
using NovaLine.Script.Editor.Window.Command;
using NovaLine.Script.Editor.Window.Context.Edge;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Interface;
using UnityEditor;
using UnityEngine;
using static NovaLine.Script.NovaElementRegistry;
using static NovaLine.Script.Editor.Window.ContextRegistry;

namespace NovaLine.Script.Editor.Utils
{
    public static class InspectorHelper
    {
        public static NovaElementInspectorWrapper InspectorNovaElementWrapper { get; set; }
        private static GlobalReplaceCoroutine ReplaceCoroutine { get; } = new();
        
        private static readonly Dictionary<string, INovaData> elementDataCache = new();

        private static int _inspectorUpdateVersion;
        
        private static bool _suppressValueChange;
        
        public static void SetSuppressValueChange(bool suppress) => _suppressValueChange = suppress;
        public static async void ShowInInspector(this NovaElement novaElement)
        {
            try
            {
                if (_suppressValueChange) return;
                int currentVersion = ++_inspectorUpdateVersion;

                await Task.Delay(50);

                if (currentVersion != _inspectorUpdateVersion) return;

                if (novaElement == null)
                {
                    Selection.activeObject = null;
                    InspectorNovaElementWrapper = null;
                    elementDataCache.Clear();
                    return;
                }

                InspectorNovaElementWrapper = NovaElementInspectorWrapper.CreateInstance(novaElement.Guid);
                SnapshotInspectorElementData();
                Selection.activeObject = InspectorNovaElementWrapper;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void OnInspectorObjValueChange()
        {
            if (InspectorNovaElementWrapper == null) return;
            var parents = InspectorNovaElementWrapper.ParentElementGuidList;
            for (int i = parents.Count - 1; i >= 0; i--)
            {
                var parentGuid = parents[i];
                var oldParent = elementDataCache[parentGuid]?.LinkedElement;
                var newParent = InspectorNovaElementWrapper.FindParent(parentGuid);
                if (!oldParent.InspectorEquals(newParent))
                {
                    TryGlobalReplace(oldParent, newParent);
                }
            }

            var selectedGuid = InspectorNovaElementWrapper.SelectedElementGuid;
            var oldSelected = elementDataCache[selectedGuid]?.LinkedElement;
            var newSelected = InspectorNovaElementWrapper.selectedElement;
            if (!oldSelected.InspectorEquals(newSelected))
            {
                TryGlobalReplace(oldSelected, newSelected);
            }
        }
        
        private static void SnapshotInspectorElementData()
        {
            if (ReplaceCoroutine.InRoutine)
            {
                ReplaceCoroutine.Stop();
                ReplaceCoroutine.ReplaceDirectly();
            }
            
            elementDataCache.Clear();
            if (InspectorNovaElementWrapper == null) return;

            //Snapshot parent elements' data
            foreach (var el in InspectorNovaElementWrapper.parentElements)
            {
                SnapshotSingleInspectorElementData(el);
            }

            //Snapshot selected element's data
            var sel = InspectorNovaElementWrapper.selectedElement;
            SnapshotSingleInspectorElementData(sel);
        }

        private static void SnapshotSingleInspectorElementData(NovaElement activeElement)
        {
            if (activeElement == null || string.IsNullOrEmpty(activeElement.Guid) || activeElement.Type == NovaElementType.None) return;
            elementDataCache[activeElement.Guid] = GetContext(activeElement.Guid,activeElement.Type)?.LinkedData?.StrongCopy();
        }

        public static void GlobalReplace(NovaElement beforeElement, NovaElement afterElement,bool registerCommand = true)
        {
            var beforeData = elementDataCache[beforeElement.Guid];
            var afterData = beforeData.StrongCopy();
            afterData.LinkedElement = afterElement;
            
            GlobalReplace(beforeData, afterData, registerCommand);
        }
        public static void GlobalReplace(INovaData beforeData, INovaData afterData, bool registerCommand = true)
        {
            void UnregisterChildContexts(IGraphViewNodeData data)
            {
                if (data == null) return;
                foreach (var nodeData in data.NodeDataList ?? Enumerable.Empty<IGraphViewNodeData>())
                {
                    if (nodeData?.LinkedElement == null) continue;
                    UnregisterContext(nodeData.Guid, nodeData.LinkedElement.Type);
                }

                foreach (var edgeData in data.EdgeDataList ?? Enumerable.Empty<IEdgeData>())
                    UnregisterContext(edgeData?.Guid, NovaElementType.Switcher);

                if (data is IAroundConditionData c)
                {
                    UnregisterContext(c.ConditionBeforeInvokeData?.Guid, NovaElementType.Condition);
                    UnregisterContext(c.ConditionAfterInvokeData?.Guid, NovaElementType.Condition);
                }
            }
            void RegisterChildContexts(IGraphViewNodeData data)
            {
                if (data == null) return;
                foreach (var nodeData in data.NodeDataList ?? Enumerable.Empty<IGraphViewNodeData>())
                {
                    if (nodeData?.LinkedElement == null) continue;
                    RegisterContext(CreateContextByType(nodeData, nodeData.LinkedElement.Type));
                }

                foreach (var edgeData in data.EdgeDataList ?? Enumerable.Empty<IEdgeData>())
                    RegisterContext(new EdgeContext(edgeData));

                if (data is IAroundConditionData c)
                {
                    if (c.ConditionBeforeInvokeData != null)
                        RegisterContext(new ConditionContext(c.ConditionBeforeInvokeData));
                    if (c.ConditionAfterInvokeData != null)
                        RegisterContext(new ConditionContext(c.ConditionAfterInvokeData));
                }
            }
            
            if(CurrentGraphViewNodeContext == null || beforeData?.LinkedElement == null || afterData?.LinkedElement == null) return;
            
            UnregisterChildContexts(beforeData as IGraphViewNodeData);
            
            afterData.LinkedElement.SetParent(beforeData.LinkedElement.Parent);
            ReplaceElement(beforeData.Guid, afterData.LinkedElement);
            
            var registeredContext = GetContext(beforeData.Guid, beforeData.Type);
            registeredContext.ReplaceLinkedData(afterData);
            
            RegisterChildContexts(afterData as IGraphViewNodeData);
            
            registeredContext.LinkedData.UpdateLinkedElement();
            
            if (registerCommand)
            {
                CommandRegistry.RegisterCommand(new InspectorElementChangeCommand(
                    CurrentGraphViewNodeContext.Guid, CurrentGraphViewNodeContext.Type,
                    beforeData, afterData));
            }

            //Re-snapshot dirty element
            SnapshotSingleInspectorElementData(afterData.LinkedElement);
            
            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }
        
        private static void TryGlobalReplace(NovaElement beforeElement, NovaElement afterElement,bool registerCommand = true)
        {
            ReplaceCoroutine?.Start(beforeElement,afterElement, registerCommand);
        }

        private class GlobalReplaceCoroutine
        {
            public NovaElement BeforeElement { get; private set; }
            public NovaElement AfterElement { get; private set; }
            public bool RegisterCommand { get; private set; }
            
            private CancellationTokenSource _cts;
            private readonly object _lock = new();
            
            public bool InRoutine
            {
                get
                {
                    lock (_lock)
                    {
                        return _cts != null && !_cts.Token.IsCancellationRequested;
                    }
                }
            }

            public void Stop()
            {
                lock (_lock)
                {
                    _cts?.Cancel();
                }
            }
            
            public void Start(NovaElement beforeElement, NovaElement afterElement, bool registerCommand = true)
            {
                if (beforeElement == null || afterElement == null) return;
                
                Stop();
                
                lock (_lock)
                {
                    _cts?.Dispose();
                    
                    _cts = new CancellationTokenSource();
                    
                    BeforeElement = beforeElement;
                    AfterElement = afterElement;
                    RegisterCommand = registerCommand;
                    
                    var token = _cts.Token;
                    
                    _ = RoutineAsync(token); 
                }
            }

            public void ReplaceDirectly()
            {
                GlobalReplace(BeforeElement, AfterElement, RegisterCommand);
            }
            
            private async Task RoutineAsync(CancellationToken token)
            {
                try
                {
                    await Task.Delay(500, token);
                    
                    token.ThrowIfCancellationRequested();
                    
                    if (BeforeElement != null && AfterElement != null)
                    {
                        ReplaceDirectly();
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    lock (_lock)
                    {
                        _cts?.Dispose();
                        _cts = null;
                    }
                }
            }
        }
    }
}