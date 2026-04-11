using System;
using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element;
using UnityEngine;

namespace NovaLine.Script.Editor.Window.Command
{
    [Serializable]
    public class InspectorElementChangeCommand : Command
    {
        public string targetElementGuid;
        public NovaElementType targetElementType;

        public string beforeJson;
        public string afterJson;
    
        public string beforeTypeName;
        public string afterTypeName;

        public InspectorElementChangeCommand(
            string contextGuid, NovaElementType contextType,
            string targetGuid,  NovaElementType targetType,
            string beforeJson,  string afterJson,
            string beforeTypeName, string afterTypeName)
            : base(contextGuid, contextType)
        {
            type = CommandType.Inspector_Change;
            targetElementGuid = targetGuid;
            targetElementType = targetType;
            this.beforeJson   = beforeJson;
            this.afterJson    = afterJson;
            this.beforeTypeName = beforeTypeName;
            this.afterTypeName  = afterTypeName;
        }

        public override void onUndo() => applyState(beforeJson, beforeTypeName);
        public override void onRedo() => applyState(afterJson, afterTypeName);

        public override void merge(Command congenericCommand)
        {
            if (congenericCommand is not InspectorElementChangeCommand other) return;
        
            if (this.afterTypeName != other.afterTypeName || this.beforeTypeName != other.beforeTypeName) return;

            afterJson = other.afterJson;
        }
    
        private void applyState(string json, string targetTypeName)
        {
            var context = ContextRegistry.GetContext(targetElementGuid, targetElementType);
            var liveElement = context?.linkedData?.linkedElement;
            if (liveElement == null) return;

            Type targetType = Type.GetType(targetTypeName);

            if (targetType != null && liveElement.GetType() != targetType)
            {
                NovaElement newInstance = (NovaElement)Activator.CreateInstance(targetType);
                JsonUtility.FromJsonOverwrite(json, newInstance);
                
                NovaElementRegistry.ReplaceElement(liveElement.guid,newInstance);
                ContextRegistry.ReplaceLinkedElement(newInstance);
                
                InspectorHelper.UpdateCacheForSwappedElement(newInstance);
            }
            else
            {
                JsonUtility.FromJsonOverwrite(json, liveElement);

                var parentContext = liveElement.parent != null
                    ? ContextRegistry.GetContext(liveElement.parent.guid, liveElement.parent.type)
                    : null;

                if (parentContext is not IGraphViewNodeContext graphViewNodeContext) return;

                if (graphViewNodeContext.graphView != null)
                {
                    var graphNode = graphViewNodeContext.graphView.getExistingGraphNode(liveElement.guid, 1);
                    if (graphNode != null) graphNode.linkedElementGuid = liveElement.guid;

                    graphViewNodeContext.graphView.update();
                }

                graphViewNodeContext.graphView?.update();
            }
            liveElement.ShowInInspector();
        }
    }
}