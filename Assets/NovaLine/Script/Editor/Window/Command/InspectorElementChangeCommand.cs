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
            Type = CommandType.Inspector_Change;
            targetElementGuid = targetGuid;
            targetElementType = targetType;
            this.beforeJson   = beforeJson;
            this.afterJson    = afterJson;
            this.beforeTypeName = beforeTypeName;
            this.afterTypeName  = afterTypeName;
        }

        public override void OnUndo() => ApplyState(beforeJson, beforeTypeName);
        public override void OnRedo() => ApplyState(afterJson, afterTypeName);

        public override void Merge(Command congenericCommand)
        {
            if (congenericCommand is not InspectorElementChangeCommand other) return;
        
            if (afterTypeName != other.afterTypeName || beforeTypeName != other.beforeTypeName) return;

            afterJson = other.afterJson;
        }
    
        private void ApplyState(string json, string targetTypeName)
        {
            var context = ContextRegistry.GetContext(targetElementGuid, targetElementType);
            var liveElement = context?.LinkedData?.linkedElement;
            if (liveElement == null) return;

            Type targetType = System.Type.GetType(targetTypeName);

            if (targetType != null && liveElement.GetType() != targetType)
            {
                NovaElement newInstance = (NovaElement)Activator.CreateInstance(targetType);
                JsonUtility.FromJsonOverwrite(json, newInstance);
                
                NovaElementRegistry.ReplaceElement(liveElement.Guid,newInstance);
                ContextRegistry.ReplaceLinkedElementInContext(newInstance);
            }
            else
            {
                JsonUtility.FromJsonOverwrite(json, liveElement);

                var parentContext = liveElement.Parent != null
                    ? ContextRegistry.GetContext(liveElement.Parent.Guid, liveElement.Parent.Type)
                    : null;

                if (parentContext is not IGraphViewNodeContext graphViewNodeContext) return;

                if (graphViewNodeContext.GraphView != null)
                {
                    var graphNode = graphViewNodeContext.GraphView.GetExistingGraphNode(liveElement.Guid, 1);
                    if (graphNode != null) graphNode.linkedElementGuid = liveElement.Guid;

                    graphViewNodeContext.GraphView.Update();
                }

                graphViewNodeContext.GraphView?.Update();
            }
            liveElement.ShowInInspector();
        }
    }
}