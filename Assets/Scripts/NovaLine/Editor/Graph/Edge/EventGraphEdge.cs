
namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Event;
    using NovaLine.Switcher;
    using UnityEngine;

    public class EventGraphEdge : GraphEdge<NovaEvent, EventSwitcher>
    {
        protected override Color themedColor => Color.yellow;
        public override EventSwitcher generateNewLinkedElement()
        {
            linkedElement = new EventSwitcher();
            return linkedElement;
        }
    }
}
