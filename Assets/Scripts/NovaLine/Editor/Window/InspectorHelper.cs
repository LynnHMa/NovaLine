using NovaLine.Editor.Utils;
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
        public static NovaElement cachedNovaElement;
        public static void ShowInInspector(this NovaElement novaElement)
        {
            if (novaElement == null)
            {
                Selection.activeObject = null;
                cachedNovaElement = null;
                wrapper = null;
                return;
            }

            wrapper = ObjectInspectorWrapper.CreateInstance(novaElement);

            cachedNovaElement = novaElement.copy();
            
            Selection.activeObject = wrapper;
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
                    }
                    parentContext.graphView.selectGraphNode(targetGraphNode);
                }
            }
        }
        public static void OnInspectorObjValueChange()
        {
            if(wrapper == null) return;
            
            CurrentGraphViewContext?.graphView?.update();
            NovaWindow.UpdateContext();
            
            var inspectorElement = wrapper.selectedElement as NovaElement;
            if (inspectorElement == null || inspectorElement.parent == null) return;
            CommandRegistry.Register(new InspectorElementChangeCommand(inspectorElement.parent.guid,inspectorElement.parent.type,cachedNovaElement,inspectorElement));
            UpdateCache();
        }
        public static void UpdateCache()
        {
            if(wrapper == null) return;
            var wrapperElement = wrapper.selectedElement as NovaElement;
            if (wrapperElement == null) return;
            cachedNovaElement = wrapperElement.copy();
        }
    }
}
