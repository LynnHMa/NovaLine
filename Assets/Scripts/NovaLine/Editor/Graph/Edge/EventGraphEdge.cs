
using NovaLine.Element.Event;

namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Editor.Utils;
    using NovaLine.Switcher;
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
