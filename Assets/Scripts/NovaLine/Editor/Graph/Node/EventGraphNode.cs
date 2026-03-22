using NovaLine.Editor.Graph.Edge;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using NovaLine.Editor.Graph.Port;
using NovaLine.Switcher;
using NovaLine.Editor.Utils;
using NovaLine.Element.Event;

namespace NovaLine.Editor.Graph.Node
{
    public class EventGraphNode : GraphNode
    {
        protected override Color themedColor => ColorExt.EVENT_THEMED_COLOR;
        public EventGraphNode(NovaEvent novaEvent, Vector2 pos) : base(novaEvent, pos)
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
