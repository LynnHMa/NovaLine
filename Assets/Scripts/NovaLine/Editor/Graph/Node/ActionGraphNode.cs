
using NovaLine.Action;
using NovaLine.Editor.Graph.Edge;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NovaLine.Editor.Graph.Node
{
    using NovaLine.Editor.Utils;
    using NovaLine.Editor.Graph.Port;
    using NovaLine.Switcher;
    using NovaLine.Editor.Graph.View;
    using System.Linq;

    [Serializable]
    public class ActionGraphNode : GraphNode
    {
        protected override Color themedColor => ColorExt.orange;
        public ActionGraphNode(NovaAction action, Vector2 pos) : base(action, pos)
        {
        }
        public override string getType()
        {
            return "[Action]";
        }
        public override void addPort()
        {
            if (linkedElement is not NovaAction action) return;

            if (action.type == ActionType.Meanwhile) return;

            var input = GraphPort<NovaAction,ActionSwitcher>.Create<ActionGraphEdge>(Orientation.Horizontal, Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), action, themedColor);

            input.portName = "In";

            var output = GraphPort<NovaAction,ActionSwitcher>.Create<ActionGraphEdge>(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), action, themedColor);

            output.portName = "Out";

            inputContainer.Add(input);
            outputContainer.Add(output);

            RefreshExpandedState();
            RefreshPorts();
        }
        public override void removePort()
        {
            if (inputContainer.childCount > 0 && outputContainer.childCount > 0)
            {
                var input = inputContainer[0] as GraphPort<NovaAction, ActionSwitcher>;
                var output = outputContainer[0] as GraphPort<NovaAction, ActionSwitcher>;
                var currentGraphView = NovaGraphWindow.getMainWindowInstance()?.currentOpenedGraphView?.graphView as NodeGraphView;
                if (currentGraphView != null)
                {
                    var inputConnections = input.connections.ToList();
                    for (var i = inputConnections.Count() - 1; i >= 0; i--)
                    {
                        var ei = inputConnections[i];
                        if (ei != null)
                        {
                            ei.input.DisconnectAll();
                            ei.output.DisconnectAll();
                            currentGraphView.RemoveElement(ei);
                        }
                    }

                    var outputConnections = output.connections.ToList();
                    for (var i = outputConnections.Count() - 1; i >= 0; i--)
                    {
                        var eo = outputConnections[i];
                        if (eo != null)
                        {
                            eo.input.DisconnectAll();
                            eo.output.DisconnectAll();
                            currentGraphView.RemoveElement(eo);
                        }
                    }
                }

                base.removePort();
            }
        }
    }
}
