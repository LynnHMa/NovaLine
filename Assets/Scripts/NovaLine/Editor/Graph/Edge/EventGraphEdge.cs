
namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Event;
    using NovaLine.Interface;
    using NovaLine.Switcher;

    public class EventGraphEdge : GraphEdge<NovaEvent, EventSwitcher>
    {
        public override void generateNewLinkedElement()
        {
            linkedElement = new EventSwitcher();
        }
    }
}
