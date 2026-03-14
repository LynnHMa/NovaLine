
namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Event;
    using NovaLine.Switcher;
    using UnityEngine;

    public class EventGraphEdge : GraphEdge<NovaEvent, EventSwitcher>
    {
        protected override Color themedColor => Color.blue;
        public EventGraphEdge() : base()
        {
        }
        public override EventSwitcher generateNewLinkedElement()
        {
            linkedElement = new EventSwitcher();
            return linkedElement;
        }
    }
}
