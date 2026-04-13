using NovaLine.Script.Editor.Graph.Edge;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using NovaLine.Script.Editor.Graph.Port;
using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Element;
using NovaLine.Script.Element.Event;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Editor.Graph.Node
{
    public class EventGraphNode : GraphNode
    {
        public override Color ThemedColor => ColorExt.EVENT_THEMED_COLOR;
        public EventGraphNode(NovaElement linkedElement, Vector2 pos) : base(linkedElement, pos)
        {
            AddPort();
        }
        public override string GetType()
        {
            return "[Event]";
        }
        public override void AddPort()
        {
            if (linkedElement is not NovaEvent novaEvent) return;

            var input = GraphPort<NovaEvent,EventSwitcher>.Create<EventGraphEdge>(Orientation.Horizontal, Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), novaEvent, ThemedColor,"In");
            var output = GraphPort<NovaEvent, EventSwitcher>.Create<EventGraphEdge>(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), novaEvent, ThemedColor,"Out");

            inputContainer.Add(input);
            outputContainer.Add(output);

            base.AddPort();
        }
    }
}
