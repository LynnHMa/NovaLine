
using NovaLine.Script.Element.Event;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Editor.Graph.Edge
{
    using NovaLine.Script.Editor.Utils;
    using UnityEngine;

    public class EventGraphEdge : GraphEdge<NovaEvent, EventSwitcher>
    {
        protected override Color ThemedColor => ColorExt.EVENT_THEMED_COLOR;
        public EventGraphEdge()
        {
        }
        public override EventSwitcher GenerateNewLinkedElement()
        {
            LinkedElement = new EventSwitcher();
            return LinkedElement;
        }
    }
}
