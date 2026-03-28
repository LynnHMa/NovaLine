using System;
using System.Threading.Tasks;
using NovaLine.Editor.Utils;
using NovaLine.Editor.Utils.Scope;
using NovaLine.Editor.Window.Command;
using NovaLine.Element;
using UnityEditor;
using UnityEngine;
using static NovaLine.Editor.Window.WindowContextRegistry;

namespace NovaLine.Editor.Window
{
    public static class InspectorHelper
    {
        private static ObjectInspectorWrapper wrapper;
        private static bool showingInInspector;
        public static NovaElement cachedNovaElement;

        private static int _inspectorUpdateVersion = 0;

        public static async void ShowInInspector(this NovaElement novaElement)
        {
            int currentVersion = ++_inspectorUpdateVersion;
            
            await Task.Delay(50);
            
            if (currentVersion != _inspectorUpdateVersion)
            {
                return;
            }
            
            try
            {
                if (novaElement == null)
                {
                    Selection.activeObject = null;
                    wrapper = null;
                    cachedNovaElement = null;
                    return;
                }
                
                wrapper = ObjectInspectorWrapper.CreateInstance(novaElement);
                cachedNovaElement = novaElement.strongCopy();
                Selection.activeObject = wrapper; 
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static void ReplaceToContext(this NovaElement novaElement)
        {
            var linkedContext = GetContext(novaElement.guid, novaElement.type);
            if (linkedContext?.linkedData != null)
            {
                linkedContext.linkedData.linkedElement = novaElement;
                
                if (linkedContext?.graphView != null)
                {
                    linkedContext.graphView.linkedElement = novaElement;
                }
                
                var parentContext = GetContext(novaElement.parent.guid, novaElement.parent.type);
                if (parentContext != null && parentContext.graphView != null)
                {
                    var targetGraphNode = parentContext.graphView.getExistingGraphNode(novaElement.guid, 1);
                    if (targetGraphNode != null)
                    {
                        targetGraphNode.linkedElement = novaElement;
                        novaElement.ShowInInspector();
                    }
                    parentContext.graphView.selectGraphNode(targetGraphNode);
                }
            }
        }
        public static void OnInspectorObjValueChange()
        {
            if(wrapper == null) return;
            var inspectorElement = wrapper.selectedElement as NovaElement;
            if (inspectorElement == null || inspectorElement.parent == null) return;
            CommandRegistry.Register(new InspectorElementChangeCommand(inspectorElement.parent.guid,inspectorElement.parent.type,cachedNovaElement.strongCopy(),inspectorElement.strongCopy()));
            UpdateCache();
            
            UpdateScope.RequireUpdate();
        }
        public static void UpdateCache()
        {
            if(wrapper == null) return;
            var wrapperElement = wrapper.selectedElement as NovaElement;
            if (wrapperElement == null) return;
            cachedNovaElement = wrapperElement.strongCopy();
            cachedNovaElement.parent = wrapperElement.parent;
        }
        
    }
}
