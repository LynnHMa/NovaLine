
using NovaLine.Action;
using NovaLine.Editor.Graph.Edge;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NovaLine.Editor.Graph.Node
{
    using NovaLine.Editor.Graph.Port;
    using NovaLine.Element;
    using NovaLine.Switcher;

    [Serializable]
    public class ActionGraphNode : GraphNode
    {
        public ActionGraphNode(NovaAction action, Vector2 pos) : base(action, pos)
        {
        }
        public override string getType()
        {
            return "[Action]";
        }
        protected override void addPort(INovaElement element)
        {
            if (element is not NovaAction action) return;

            var input = GraphPort<NovaAction,ActionSwitcher>.Create<ActionGraphEdge>(Orientation.Horizontal, Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), action);

            input.portName = "In";

            var output = GraphPort<NovaAction,ActionSwitcher>.Create<ActionGraphEdge>(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), action);

            output.portName = "Out";

            inputContainer.Add(input);
            outputContainer.Add(output);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
