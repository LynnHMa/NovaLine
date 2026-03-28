
using NovaLine.Element.Event;
using NovaLine.Element.Switcher;

namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Editor.Utils;
    using UnityEngine;

    public class EventGraphEdge : GraphEdge<NovaEvent, EventSwitcher>
    {
        protected override Color themedColor => ColorExt.EVENT_THEMED_COLOR;
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
