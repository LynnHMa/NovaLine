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
        protected override Color themedColor => ColorExt.EVENT_THEMED_COLOR;
        public EventGraphNode(NovaElement linkedElement, Vector2 pos) : base(linkedElement, pos)
        {
            addPort();
        }
        public override string getType()
        {
            return "[Event]";
        }
        public override void addPort()
        {
            if (linkedElement is not NovaEvent novaEvent) return;

            var input = GraphPort<NovaEvent,EventSwitcher>.Create<EventGraphEdge>(Orientation.Horizontal, Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), novaEvent, themedColor,"In");
            var output = GraphPort<NovaEvent, EventSwitcher>.Create<EventGraphEdge>(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), novaEvent, themedColor,"Out");

            inputContainer.Add(input);
            outputContainer.Add(output);

            base.addPort();
        }
    }
}
