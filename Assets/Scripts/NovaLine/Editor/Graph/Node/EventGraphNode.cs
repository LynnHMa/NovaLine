
using NovaLine.Action;
using NovaLine.Editor.Graph.Edge;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using NovaLine.Editor.Graph.Port;
using NovaLine.Switcher;
using NovaLine.Event;
using NovaLine.Element;

namespace NovaLine.Editor.Graph.Node
{
    public class EventGraphNode : GraphNode
    {
        protected override Color themedColor => Color.blue;
        public EventGraphNode(NovaEvent novaEvent, Vector2 pos) : base(novaEvent, pos)
        {
        }
        public override string getType()
        {
            return "[Event]";
        }
        public override void addPort()
        {
            if (linkedElement is not NovaEvent novaEvent) return;
            if (linkedElement?.parent is not Condition condition) return;

            if (condition.type != ConditionType.Sort) return;

            var input = GraphPort<NovaEvent,EventSwitcher>.Create<EventGraphEdge>(Orientation.Horizontal, Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), novaEvent, themedColor,"In");
            var output = GraphPort<NovaEvent, EventSwitcher>.Create<EventGraphEdge>(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), novaEvent, themedColor,"Out");

            inputContainer.Add(input);
            outputContainer.Add(output);

            base.addPort();
        }
        public override void update()
        {
            base.update();
            if (linkedElement?.parent is Condition condition)
            {
                if (condition.type == ConditionType.Sort && inputContainer.childCount == 0)
                {
                    addPort();
                }
                else if(condition.type != ConditionType.Sort && inputContainer.childCount > 0)
                {
                    removePort();
                }
            }
        }
    }
}
